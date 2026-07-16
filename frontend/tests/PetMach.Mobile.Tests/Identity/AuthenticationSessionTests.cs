using FluentAssertions;
using PetMach.Mobile.Core.Identity;

namespace PetMach.Mobile.Tests.Identity;

public sealed class AuthenticationSessionTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 14, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExpiringAccessTokenShouldBeRotatedAndStored()
    {
        MemoryTokenStore store = new()
        {
            Session = new StoredSession("old-access", Now.AddSeconds(30), "old-refresh", Now.AddDays(1)),
        };
        FakeAuthApi api = new();
        AuthenticationSession session = new(api, store, new FixedTimeProvider(Now));

        string? accessToken = await session.GetAccessTokenAsync(CancellationToken.None);

        accessToken.Should().Be("new-access");
        store.Session!.RefreshToken.Should().Be("new-refresh");
        api.RefreshCalls.Should().Be(1);
    }

    [Fact]
    public async Task ExpiredRefreshTokenShouldClearSession()
    {
        MemoryTokenStore store = new()
        {
            Session = new StoredSession("access", Now.AddMinutes(5), "refresh", Now.AddSeconds(-1)),
        };
        AuthenticationSession session = new(new FakeAuthApi(), store, new FixedTimeProvider(Now));

        string? accessToken = await session.GetAccessTokenAsync(CancellationToken.None);

        accessToken.Should().BeNull();
        store.Session.Should().BeNull();
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class MemoryTokenStore : ITokenStore
    {
        public StoredSession? Session { get; set; }
        public Task<StoredSession?> GetAsync(CancellationToken cancellationToken) => Task.FromResult(Session);
        public Task SaveAsync(StoredSession session, CancellationToken cancellationToken) { Session = session; return Task.CompletedTask; }
        public Task ClearAsync(CancellationToken cancellationToken) { Session = null; return Task.CompletedTask; }
    }

    private sealed class FakeAuthApi : IAuthApiClient
    {
        public int RefreshCalls { get; private set; }
        public Task<RegistrationResult> RegisterAsync(RegistrationInput input, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<TokenEnvelope> LoginAsync(LoginInput input, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<TokenEnvelope> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
        {
            RefreshCalls++;
            return Task.FromResult(new TokenEnvelope("new-access", Now.AddMinutes(15), "new-refresh", Now.AddDays(30)));
        }
        public Task LogoutAsync(string accessToken, string refreshToken, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
