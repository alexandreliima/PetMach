using FluentAssertions;
using PetMach.Mobile.Core.Identity;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Tests.Identity;

public sealed class AppStartupCoordinatorTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 17, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task UserWithoutSessionShouldOpenPublicRoot()
    {
        RecordingRootNavigation roots = new();
        AppStartupCoordinator startup = new(CreateSession(new MemoryTokenStore()), roots);

        await startup.InitializeAsync(TestCancellation.Token);

        roots.PublicRoots.Should().HaveCount(1);
        roots.AuthenticatedRoots.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidSessionShouldOpenAuthenticatedRoot()
    {
        MemoryTokenStore store = new()
        {
            Session = new StoredSession("access", Now.AddMinutes(10), "refresh", Now.AddDays(1)),
        };
        RecordingRootNavigation roots = new();
        AppStartupCoordinator startup = new(CreateSession(store), roots);

        await startup.InitializeAsync(TestCancellation.Token);

        roots.AuthenticatedRoots.Should().ContainSingle()
            .Which.Route.Should().Be("//app/network");
        roots.PublicRoots.Should().BeEmpty();
    }

    [Fact]
    public async Task SecureStorageFailureShouldOpenPublicRootWithoutCrash()
    {
        RecordingRootNavigation roots = new();
        AuthenticationSession session = CreateSession(new FailingTokenStore());
        AppStartupCoordinator startup = new(session, roots);

        await startup.InitializeAsync(TestCancellation.Token);

        roots.PublicRoots.Should().HaveCount(1);
        roots.AuthenticatedRoots.Should().BeEmpty();
    }

    [Fact]
    public async Task RepeatedPublicRequestsShouldProduceDistinctRootInstances()
    {
        RecordingRootNavigation roots = new();

        await roots.ShowPublicRootAsync(TestCancellation.Token);
        await roots.ShowPublicRootAsync(TestCancellation.Token);

        roots.PublicRoots.Should().HaveCount(2);
        roots.PublicRoots[0].Should().NotBeSameAs(roots.PublicRoots[1]);
    }

    private static AuthenticationSession CreateSession(ITokenStore store) =>
        new(new UnusedAuthApi(), store, new FixedTimeProvider(Now));

    private sealed class RecordingRootNavigation : IRootNavigationService
    {
        public List<object> PublicRoots { get; } = [];
        public List<(object Root, string Route)> AuthenticatedRoots { get; } = [];

        public Task ShowPublicRootAsync(CancellationToken cancellationToken)
        {
            PublicRoots.Add(new object());
            return Task.CompletedTask;
        }

        public Task ShowAuthenticatedRootAsync(string route, CancellationToken cancellationToken)
        {
            AuthenticatedRoots.Add((new object(), route));
            return Task.CompletedTask;
        }
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

    private sealed class FailingTokenStore : ITokenStore
    {
        public Task<StoredSession?> GetAsync(CancellationToken cancellationToken) =>
            throw new InvalidOperationException("SecureStorage indisponível.");
        public Task SaveAsync(StoredSession session, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task ClearAsync(CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class UnusedAuthApi : IAuthApiClient
    {
        public Task<RegistrationResult> RegisterAsync(RegistrationInput input, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task<TokenEnvelope> LoginAsync(LoginInput input, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task<TokenEnvelope> RefreshAsync(string refreshToken, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task LogoutAsync(string accessToken, string refreshToken, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
