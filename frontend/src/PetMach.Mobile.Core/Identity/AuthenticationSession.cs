using System.Net;

namespace PetMach.Mobile.Core.Identity;

public sealed class AuthenticationSession(
    IAuthApiClient api,
    ITokenStore tokenStore,
    TimeProvider timeProvider)
{
    private static readonly TimeSpan AccessTokenSafetyWindow = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan SharedOperationTimeout = TimeSpan.FromSeconds(30);
    private readonly object synchronization = new();
    private Task<string?>? refreshTask;
    private Task? invalidationTask;

    public event Func<CancellationToken, Task>? Invalidated;

    public async Task LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        TokenEnvelope tokens = await api.LoginAsync(new LoginInput(email, password), cancellationToken);
        await tokenStore.SaveAsync(ToStored(tokens), cancellationToken);
        lock (synchronization)
        {
            invalidationTask = null;
        }
    }

    public async Task<bool> TryRestoreAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await GetAccessTokenAsync(cancellationToken) is not null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        StoredSession? session = await tokenStore.GetAsync(cancellationToken);
        if (session is null)
        {
            return null;
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        if (session.RefreshTokenExpiresAtUtc <= now)
        {
            await InvalidateOnceAsync(cancellationToken);
            return null;
        }

        if (session.AccessTokenExpiresAtUtc > now.Add(AccessTokenSafetyWindow))
        {
            return session.AccessToken;
        }

        return await AwaitSharedRefreshAsync(session.RefreshToken, cancellationToken);
    }

    public async Task<string?> RefreshAfterUnauthorizedAsync(
        string rejectedAccessToken,
        CancellationToken cancellationToken)
    {
        StoredSession? session = await tokenStore.GetAsync(cancellationToken);
        if (session is null)
        {
            return null;
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        if (!string.Equals(session.AccessToken, rejectedAccessToken, StringComparison.Ordinal) &&
            session.AccessTokenExpiresAtUtc > now)
        {
            return session.AccessToken;
        }

        if (session.RefreshTokenExpiresAtUtc <= now)
        {
            await InvalidateOnceAsync(cancellationToken);
            return null;
        }

        return await AwaitSharedRefreshAsync(session.RefreshToken, cancellationToken);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        StoredSession? session = await tokenStore.GetAsync(cancellationToken);
        try
        {
            if (session is not null)
            {
                await api.LogoutAsync(session.AccessToken, session.RefreshToken, cancellationToken);
            }
        }
        finally
        {
            using CancellationTokenSource cleanupTimeout = new(SharedOperationTimeout);
            await tokenStore.ClearAsync(cleanupTimeout.Token);
        }
    }

    private async Task<string?> AwaitSharedRefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        Task<string?> sharedTask;
        lock (synchronization)
        {
            if (refreshTask is null || refreshTask.IsCompleted)
            {
                refreshTask = RefreshCoreAsync(refreshToken);
            }

            sharedTask = refreshTask;
        }

        return await sharedTask.WaitAsync(cancellationToken);
    }

    private async Task<string?> RefreshCoreAsync(string expectedRefreshToken)
    {
        using CancellationTokenSource timeout = new(SharedOperationTimeout);
        try
        {
            StoredSession? current = await tokenStore.GetAsync(timeout.Token);
            if (current is null)
            {
                return null;
            }

            if (!string.Equals(current.RefreshToken, expectedRefreshToken, StringComparison.Ordinal))
            {
                return current.AccessToken;
            }

            TokenEnvelope refreshed = await api.RefreshAsync(expectedRefreshToken, timeout.Token);
            await tokenStore.SaveAsync(ToStored(refreshed), timeout.Token);
            return refreshed.AccessToken;
        }
        catch (AuthenticationApiException exception) when (IsDefinitiveRefreshFailure(exception.StatusCode))
        {
            await InvalidateOnceAsync(timeout.Token);
            return null;
        }
    }

    private Task InvalidateOnceAsync(CancellationToken cancellationToken)
    {
        Task sharedTask;
        lock (synchronization)
        {
            invalidationTask ??= InvalidateCoreAsync();
            sharedTask = invalidationTask;
        }

        return sharedTask.WaitAsync(cancellationToken);
    }

    private async Task InvalidateCoreAsync()
    {
        using CancellationTokenSource timeout = new(SharedOperationTimeout);
        try
        {
            await tokenStore.ClearAsync(timeout.Token);
        }
        finally
        {
            Func<CancellationToken, Task>? handler = Invalidated;
            if (handler is not null)
            {
                await handler(timeout.Token);
            }
        }
    }

    private static bool IsDefinitiveRefreshFailure(HttpStatusCode? statusCode) =>
        statusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden;

    private static StoredSession ToStored(TokenEnvelope tokens) =>
        new(
            tokens.AccessToken,
            tokens.AccessTokenExpiresAtUtc,
            tokens.RefreshToken,
            tokens.RefreshTokenExpiresAtUtc);
}
