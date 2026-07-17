using System.Net;
using FluentAssertions;
using PetMach.Mobile.Core.Identity;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Tests.Identity;

public sealed class SessionLifecycleCoordinatorTests
{
    [Fact]
    public async Task InvalidRefreshShouldStopConnectionsAndReturnToPublicRoot()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        MemoryTokenStore store = new()
        {
            Session = new StoredSession(
                "access",
                now.AddSeconds(10),
                "refresh",
                now.AddDays(1)),
        };
        AuthenticationSession session = new(
            new InvalidRefreshApi(),
            store,
            new FixedTimeProvider(now));
        RecordingConnections connections = new();
        RecordingRoots roots = new();
        SessionLifecycleCoordinator lifecycle = new(session, connections, roots);
        lifecycle.Start();

        string? token = await session.GetAccessTokenAsync(TestCancellation.Token);

        token.Should().BeNull();
        connections.StopCalls.Should().Be(1);
        roots.PublicCalls.Should().Be(1);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class MemoryTokenStore : ITokenStore
    {
        public StoredSession? Session { get; set; }
        public Task<StoredSession?> GetAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Session);
        public Task SaveAsync(StoredSession session, CancellationToken cancellationToken) =>
            Task.CompletedTask;
        public Task ClearAsync(CancellationToken cancellationToken)
        {
            Session = null;
            return Task.CompletedTask;
        }
    }

    private sealed class InvalidRefreshApi : IAuthApiClient
    {
        public Task<RegistrationResult> RegisterAsync(RegistrationInput input, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task<TokenEnvelope> LoginAsync(LoginInput input, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task<TokenEnvelope> RefreshAsync(string refreshToken, CancellationToken cancellationToken) =>
            throw new AuthenticationApiException(
                "identity.refresh_invalid",
                "Refresh inválido.",
                HttpStatusCode.Unauthorized);
        public Task LogoutAsync(string accessToken, string refreshToken, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
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

    private sealed class RecordingRoots : IRootNavigationService
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
