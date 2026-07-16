namespace PetMach.Mobile.Core.Identity;

public sealed class AuthenticationSession(IAuthApiClient api, ITokenStore tokenStore, TimeProvider timeProvider)
{
    public async Task LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        TokenEnvelope tokens = await api.LoginAsync(new LoginInput(email, password), cancellationToken);
        await tokenStore.SaveAsync(ToStored(tokens), cancellationToken);
    }

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        StoredSession? session = await tokenStore.GetAsync(cancellationToken);
        if (session is null) return null;
        if (session.RefreshTokenExpiresAtUtc <= timeProvider.GetUtcNow())
        {
            await tokenStore.ClearAsync(cancellationToken);
            return null;
        }
        if (session.AccessTokenExpiresAtUtc > timeProvider.GetUtcNow().AddMinutes(1)) return session.AccessToken;
        TokenEnvelope refreshed = await api.RefreshAsync(session.RefreshToken, cancellationToken);
        await tokenStore.SaveAsync(ToStored(refreshed), cancellationToken);
        return refreshed.AccessToken;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        StoredSession? session = await tokenStore.GetAsync(cancellationToken);
        try
        {
            if (session is not null) await api.LogoutAsync(session.AccessToken, session.RefreshToken, cancellationToken);
        }
        finally
        {
            await tokenStore.ClearAsync(cancellationToken);
        }
    }

    private static StoredSession ToStored(TokenEnvelope tokens) => new(tokens.AccessToken, tokens.AccessTokenExpiresAtUtc, tokens.RefreshToken, tokens.RefreshTokenExpiresAtUtc);
}
