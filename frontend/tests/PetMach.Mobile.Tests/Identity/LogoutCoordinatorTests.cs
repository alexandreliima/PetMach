using FluentAssertions;
using PetMach.Mobile.Core.Identity;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Tests.Identity;

public sealed class LogoutCoordinatorTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 17, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task LogoutShouldClearSessionStopConnectionsAndOpenPublicRoot()
    {
        MemoryTokenStore store = StoreWithSession();
        FakeAuthApi api = new();
        RecordingConnections connections = new();
        RecordingRootNavigation roots = new();
        LogoutCoordinator logout = new(CreateSession(api, store), connections, roots);

        await logout.LogoutAsync(TestCancellation.Token);

        api.LogoutCalls.Should().Be(1);
        store.Session.Should().BeNull();
        store.ClearCalls.Should().Be(1);
        connections.StopCalls.Should().Be(1);
        roots.PublicCalls.Should().Be(1);
    }

    [Fact]
    public async Task RemoteLogoutFailureShouldStillClearLocalSessionAndOpenPublicRoot()
    {
        MemoryTokenStore store = StoreWithSession();
        FakeAuthApi api = new() { FailLogout = true };
        RecordingConnections connections = new();
        RecordingRootNavigation roots = new();
        LogoutCoordinator logout = new(CreateSession(api, store), connections, roots);

        await logout.LogoutAsync(TestCancellation.Token);

        store.Session.Should().BeNull();
        store.ClearCalls.Should().Be(1);
        connections.StopCalls.Should().Be(1);
        roots.PublicCalls.Should().Be(1);
    }

    private static AuthenticationSession CreateSession(FakeAuthApi api, ITokenStore store) =>
        new(api, store, new FixedTimeProvider(Now));

    private static MemoryTokenStore StoreWithSession() =>
        new()
        {
            Session = new StoredSession("access", Now.AddMinutes(10), "refresh", Now.AddDays(1)),
        };

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class MemoryTokenStore : ITokenStore
    {
        public StoredSession? Session { get; set; }
        public int ClearCalls { get; private set; }
        public Task<StoredSession?> GetAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Session);
        public Task SaveAsync(StoredSession session, CancellationToken cancellationToken) =>
            Task.CompletedTask;
        public Task ClearAsync(CancellationToken cancellationToken)
        {
            Session = null;
            ClearCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAuthApi : IAuthApiClient
    {
        public bool FailLogout { get; init; }
        public int LogoutCalls { get; private set; }

        public Task<RegistrationResult> RegisterAsync(RegistrationInput input, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task<TokenEnvelope> LoginAsync(LoginInput input, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task<TokenEnvelope> RefreshAsync(string refreshToken, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task LogoutAsync(string accessToken, string refreshToken, CancellationToken cancellationToken)
        {
            LogoutCalls++;
            return FailLogout
                ? Task.FromException(new HttpRequestException("API indisponível."))
                : Task.CompletedTask;
        }
    }

    private sealed class RecordingConnections : IAuthenticatedConnectionManager
    {
        public int StopCalls { get; private set; }
        public Task StopAuthenticatedConnectionsAsync(CancellationToken cancellationToken)
        {
            StopCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingRootNavigation : IRootNavigationService
    {
        public int PublicCalls { get; private set; }
        public Task ShowPublicRootAsync(CancellationToken cancellationToken)
        {
            PublicCalls++;
            return Task.CompletedTask;
        }

        public Task ShowAuthenticatedRootAsync(string route, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
