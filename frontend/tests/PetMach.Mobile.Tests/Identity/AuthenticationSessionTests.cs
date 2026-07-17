using System.Net;
using FluentAssertions;
using PetMach.Mobile.Core.Identity;

namespace PetMach.Mobile.Tests.Identity;

public sealed class AuthenticationSessionTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 14, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExpiringAccessTokenShouldBeRotatedAndStored()
    {
        MemoryTokenStore store = StoreWithExpiringAccessToken();
        FakeAuthApi api = new();
        AuthenticationSession session = CreateSession(api, store);

        string? accessToken = await session.GetAccessTokenAsync(TestCancellation.Token);

        accessToken.Should().Be("new-access");
        store.Session!.RefreshToken.Should().Be("new-refresh");
        api.RefreshCalls.Should().Be(1);
    }

    [Fact]
    public async Task ConcurrentRequestsShouldShareOneRefreshAndReceiveTheRotatedToken()
    {
        MemoryTokenStore store = StoreWithExpiringAccessToken();
        FakeAuthApi api = new() { WaitForRelease = true };
        AuthenticationSession session = CreateSession(api, store);
        CancellationToken cancellationToken = TestCancellation.Token;

        Task<string?>[] requests = Enumerable.Range(0, 8)
            .Select(_ => session.GetAccessTokenAsync(cancellationToken))
            .ToArray();
        await api.RefreshStarted.Task.WaitAsync(cancellationToken);
        api.ReleaseRefresh();

        string?[] tokens = await Task.WhenAll(requests);

        tokens.Should().OnlyContain(token => token == "new-access");
        api.RefreshCalls.Should().Be(1);
        store.SaveCalls.Should().Be(1);
    }

    [Fact]
    public async Task CancellingOneWaiterShouldNotCancelTheSharedRefresh()
    {
        MemoryTokenStore store = StoreWithExpiringAccessToken();
        FakeAuthApi api = new() { WaitForRelease = true };
        AuthenticationSession session = CreateSession(api, store);
        using CancellationTokenSource cancelledWaiter = new();

        Task<string?> first = session.GetAccessTokenAsync(cancelledWaiter.Token);
        Task<string?> second = session.GetAccessTokenAsync(TestCancellation.Token);
        await api.RefreshStarted.Task.WaitAsync(TestCancellation.Token);
        cancelledWaiter.Cancel();
        api.ReleaseRefresh();

        Func<Task> cancelledAction = async () => _ = await first;
        await cancelledAction.Should().ThrowAsync<OperationCanceledException>();
        (await second).Should().Be("new-access");
        api.RefreshCalls.Should().Be(1);
    }

    [Fact]
    public async Task DefinitiveRefreshFailureShouldClearAndNotifyOnlyOnce()
    {
        MemoryTokenStore store = StoreWithExpiringAccessToken();
        FakeAuthApi api = new()
        {
            RefreshException = new AuthenticationApiException(
                "identity.refresh_reused",
                "Refresh inválido.",
                HttpStatusCode.Unauthorized),
        };
        AuthenticationSession session = CreateSession(api, store);
        int invalidations = 0;
        session.Invalidated += _ =>
        {
            Interlocked.Increment(ref invalidations);
            return Task.CompletedTask;
        };

        string?[] tokens = await Task.WhenAll(
            Enumerable.Range(0, 5)
                .Select(_ => session.GetAccessTokenAsync(TestCancellation.Token)));

        tokens.Should().OnlyContain(token => token == null);
        api.RefreshCalls.Should().Be(1);
        store.ClearCalls.Should().Be(1);
        invalidations.Should().Be(1);
    }

    [Fact]
    public async Task ExpiredRefreshTokenShouldClearSession()
    {
        MemoryTokenStore store = new()
        {
            Session = new StoredSession("access", Now.AddMinutes(5), "refresh", Now.AddSeconds(-1)),
        };
        AuthenticationSession session = CreateSession(new FakeAuthApi(), store);

        string? accessToken = await session.GetAccessTokenAsync(TestCancellation.Token);

        accessToken.Should().BeNull();
        store.Session.Should().BeNull();
        store.ClearCalls.Should().Be(1);
    }

    [Fact]
    public async Task SecureStorageFailureDuringRestoreShouldReturnNoSession()
    {
        AuthenticationSession session = CreateSession(new FakeAuthApi(), new FailingTokenStore());

        bool restored = await session.TryRestoreAsync(TestCancellation.Token);

        restored.Should().BeFalse();
    }

    private static AuthenticationSession CreateSession(FakeAuthApi api, ITokenStore store) =>
        new(api, store, new FixedTimeProvider(Now));

    private static MemoryTokenStore StoreWithExpiringAccessToken() =>
        new()
        {
            Session = new StoredSession("old-access", Now.AddSeconds(30), "old-refresh", Now.AddDays(1)),
        };

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class MemoryTokenStore : ITokenStore
    {
        public StoredSession? Session { get; set; }
        public int SaveCalls { get; private set; }
        public int ClearCalls { get; private set; }

        public Task<StoredSession?> GetAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Session);

        public Task SaveAsync(StoredSession session, CancellationToken cancellationToken)
        {
            Session = session;
            SaveCalls++;
            return Task.CompletedTask;
        }

        public Task ClearAsync(CancellationToken cancellationToken)
        {
            Session = null;
            ClearCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FailingTokenStore : ITokenStore
    {
        public Task<StoredSession?> GetAsync(CancellationToken cancellationToken) =>
            throw new InvalidOperationException("SecureStorage indisponível.");

        public Task SaveAsync(StoredSession session, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task ClearAsync(CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class FakeAuthApi : IAuthApiClient
    {
        private readonly TaskCompletionSource refreshStarted =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource releaseRefresh =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int refreshCalls;

        public bool WaitForRelease { get; init; }
        public Exception? RefreshException { get; init; }
        public int RefreshCalls => refreshCalls;
        public TaskCompletionSource RefreshStarted => refreshStarted;

        public Task<RegistrationResult> RegisterAsync(
            RegistrationInput input,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<TokenEnvelope> LoginAsync(
            LoginInput input,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public async Task<TokenEnvelope> RefreshAsync(
            string refreshToken,
            CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref refreshCalls);
            refreshStarted.TrySetResult();
            if (WaitForRelease)
            {
                await releaseRefresh.Task.WaitAsync(cancellationToken);
            }

            if (RefreshException is not null)
            {
                throw RefreshException;
            }

            return new TokenEnvelope(
                "new-access",
                Now.AddMinutes(15),
                "new-refresh",
                Now.AddDays(30));
        }

        public Task LogoutAsync(
            string accessToken,
            string refreshToken,
            CancellationToken cancellationToken) => Task.CompletedTask;

        public void ReleaseRefresh() => releaseRefresh.TrySetResult();
    }
}
