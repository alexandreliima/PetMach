using FluentAssertions;
using PetMach.Mobile.Core.Identity;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Tests.Identity;

public sealed class LoginViewModelTests
{
    [Fact]
    public async Task SuccessfulLoginShouldStoreSessionAndRequestAuthenticatedRoot()
    {
        MemoryTokenStore store = new();
        Navigator navigator = new();
        AuthenticationSession session = new(
            new SuccessfulLoginApi(),
            store,
            TimeProvider.System);
        LoginViewModel viewModel = new(session, navigator)
        {
            Email = "tutor@example.test",
            Password = "Senha-segura-123",
        };

        await viewModel.LoginCommand.ExecuteAsync(null);

        store.Session.Should().NotBeNull();
        navigator.Route.Should().Be("//app/network");
        navigator.AuthenticatedRootId.Should().NotBeNull();
    }

    private sealed class MemoryTokenStore : ITokenStore
    {
        public StoredSession? Session { get; private set; }
        public Task<StoredSession?> GetAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Session);
        public Task SaveAsync(StoredSession session, CancellationToken cancellationToken)
        {
            Session = session;
            return Task.CompletedTask;
        }
        public Task ClearAsync(CancellationToken cancellationToken)
        {
            Session = null;
            return Task.CompletedTask;
        }
    }

    private sealed class SuccessfulLoginApi : IAuthApiClient
    {
        public Task<RegistrationResult> RegisterAsync(RegistrationInput input, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task<TokenEnvelope> LoginAsync(LoginInput input, CancellationToken cancellationToken) =>
            Task.FromResult(new TokenEnvelope(
                "access",
                DateTimeOffset.UtcNow.AddMinutes(15),
                "refresh",
                DateTimeOffset.UtcNow.AddDays(30)));
        public Task<TokenEnvelope> RefreshAsync(string refreshToken, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task LogoutAsync(string accessToken, string refreshToken, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class Navigator : IMobileNavigator
    {
        public string? Route { get; private set; }
        public object? AuthenticatedRootId { get; private set; }

        public Task GoToAsync(string route)
        {
            Route = route;
            AuthenticatedRootId = new object();
            return Task.CompletedTask;
        }
    }
}
