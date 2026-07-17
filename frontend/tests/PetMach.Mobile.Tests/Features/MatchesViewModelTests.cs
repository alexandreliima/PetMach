using FluentAssertions;
using NSubstitute;
using PetMach.Mobile.Core.Features;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Tests.Features;

public sealed class MatchesViewModelTests
{
    [Fact]
    public async Task ConversationShouldOpenTheBottomNavigationTab()
    {
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        Navigator navigator = new();
        MatchesViewModel viewModel = new(api, navigator);

        await viewModel.OpenConversationCommand.ExecuteAsync(null);

        navigator.Route.Should().Be("//app/conversations-tab");
    }

    [Fact]
    public async Task MeetingShouldCarryTheSelectedMatch()
    {
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        Navigator navigator = new();
        MatchesViewModel viewModel = new(api, navigator);
        MatchModel match = new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Lua", "SRD", DateTimeOffset.UtcNow);

        await viewModel.ProposeMeetingCommand.ExecuteAsync(match);

        navigator.Route.Should().Be($"meetings?matchId={match.Id}");
    }

    private sealed class Navigator : IMobileNavigator
    {
        public string? Route { get; private set; }

        public Task GoToAsync(string route)
        {
            Route = route;
            return Task.CompletedTask;
        }
    }
}
