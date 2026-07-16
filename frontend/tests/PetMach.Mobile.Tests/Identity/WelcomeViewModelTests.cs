using FluentAssertions;
using PetMach.Mobile.Core.Identity;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Tests.Identity;

public sealed class WelcomeViewModelTests
{
    [Fact]
    public async Task RegistrationCommandShouldNavigateToRegistration()
    {
        Navigator navigator = new();
        WelcomeViewModel viewModel = new(navigator);

        await viewModel.OpenRegistrationCommand.ExecuteAsync(null);

        navigator.Route.Should().Be("register");
    }

    [Fact]
    public async Task AboutCommandShouldOpenPublicProductPresentation()
    {
        Navigator navigator = new();
        WelcomeViewModel viewModel = new(navigator);

        await viewModel.OpenAboutCommand.ExecuteAsync(null);

        navigator.Route.Should().Be("about");
    }

    private sealed class Navigator : IMobileNavigator
    {
        public string? Route { get; private set; }
        public Task GoToAsync(string route) { Route = route; return Task.CompletedTask; }
    }
}
