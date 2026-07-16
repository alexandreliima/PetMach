using FluentAssertions;
using PetMach.Mobile.Core.Identity;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Tests.Identity;

public sealed class RegisterViewModelTests
{
    [Fact]
    public async Task SuccessfulRegistrationShouldNavigateToLogin()
    {
        Navigator navigator = new();
        RegisterViewModel viewModel = new(new SuccessfulAuthApi(), navigator)
        {
            Email = "tutor@example.test",
            Password = "Senha-segura-123",
            AcceptedTerms = true,
            AcceptedPrivacy = true,
        };

        await viewModel.RegisterCommand.ExecuteAsync(null);

        navigator.Route.Should().Be("login");
    }

    private sealed class SuccessfulAuthApi : IAuthApiClient
    {
        public Task<RegistrationResult> RegisterAsync(RegistrationInput input, CancellationToken cancellationToken) =>
            Task.FromResult(new RegistrationResult(Guid.NewGuid(), true));
        public Task<TokenEnvelope> LoginAsync(LoginInput input, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<TokenEnvelope> RefreshAsync(string refreshToken, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task LogoutAsync(string accessToken, string refreshToken, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class Navigator : IMobileNavigator
    {
        public string? Route { get; private set; }
        public Task GoToAsync(string route) { Route = route; return Task.CompletedTask; }
    }
}
