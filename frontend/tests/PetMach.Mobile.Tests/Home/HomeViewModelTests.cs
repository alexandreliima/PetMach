using FluentAssertions;
using PetMach.Mobile.Core.Home;
using PetMach.Mobile.Core.Identity;
using PetMach.Mobile.Core.Navigation;
using PetMach.Mobile.Core.Settings;

namespace PetMach.Mobile.Tests.Home;

public sealed class HomeViewModelTests
{
    [Theory]
    [InlineData("profile", "tutor-profile")]
    [InlineData("dogs", "dogs")]
    [InlineData("health", "health")]
    [InlineData("discovery", "discovery")]
    [InlineData("matches", "matches")]
    [InlineData("notifications", "notifications")]
    [InlineData("conversations", "conversations")]
    [InlineData("meetings", "meetings")]
    [InlineData("partner-spaces", "partner-spaces")]
    [InlineData("reservations", "reservations")]
    [InlineData("partner-operations", "partner-operations")]
    [InlineData("adoption", "adoption")]
    [InlineData("settings", SettingsRoutes.Settings)]
    public async Task FeatureCommandsShouldOpenTheExpectedRoute(string feature, string route)
    {
        Navigator navigator = new();
        HomeViewModel viewModel = new(navigator, new Logout());

        if (feature == "profile") await viewModel.OpenTutorProfileCommand.ExecuteAsync(null);
        else if (feature == "dogs") await viewModel.OpenDogsCommand.ExecuteAsync(null);
        else if (feature == "health") await viewModel.OpenHealthCommand.ExecuteAsync(null);
        else if (feature == "discovery") await viewModel.OpenDiscoveryCommand.ExecuteAsync(null);
        else if (feature == "matches") await viewModel.OpenMatchesCommand.ExecuteAsync(null);
        else if (feature == "notifications") await viewModel.OpenNotificationsCommand.ExecuteAsync(null);
        else if (feature == "conversations") await viewModel.OpenConversationsCommand.ExecuteAsync(null);
        else if (feature == "meetings") await viewModel.OpenMeetingsCommand.ExecuteAsync(null);
        else if (feature == "partner-spaces") await viewModel.OpenPartnerSpacesCommand.ExecuteAsync(null);
        else if (feature == "reservations") await viewModel.OpenReservationsCommand.ExecuteAsync(null);
        else if (feature == "partner-operations") await viewModel.OpenPartnerOperationsCommand.ExecuteAsync(null);
        else if (feature == "adoption") await viewModel.OpenAdoptionCommand.ExecuteAsync(null);
        else await viewModel.OpenSettingsCommand.ExecuteAsync(null);

        navigator.Route.Should().Be(route);
    }

    private sealed class Navigator : IMobileNavigator
    {
        public string? Route { get; private set; }
        public Task GoToAsync(string route) { Route = route; return Task.CompletedTask; }
    }

    private sealed class Logout : ILogoutCoordinator
    {
        public Task LogoutAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
