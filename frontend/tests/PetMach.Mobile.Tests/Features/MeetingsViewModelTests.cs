using FluentAssertions;
using NSubstitute;
using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile.Tests.Features;

public sealed class MeetingsViewModelTests
{
    [Fact]
    public async Task ProposalShouldRequireAFutureDate()
    {
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        MeetingsViewModel viewModel = new(api)
        {
            SelectedMatch = Match(),
            PlaceName = "Parque",
            ScheduledDate = DateTime.Today.AddDays(-1),
        };

        await viewModel.ProposeCommand.ExecuteAsync(null);

        viewModel.StatusMessage.Should().Be("Escolha uma data e hora futuras.");
        await api.DidNotReceiveWithAnyArgs().CreateMeetingAsync(default, default, default!, default, default);
    }

    [Fact]
    public async Task AcceptShouldReplaceMeetingWithServerState()
    {
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        MeetingModel proposed = Meeting(MeetingStatusModel.Proposed);
        MeetingModel accepted = proposed with { Status = MeetingStatusModel.Accepted, CanRespond = false };
        api.TransitionMeetingAsync(proposed.Id, "accept", Arg.Any<CancellationToken>()).Returns(accepted);
        MeetingsViewModel viewModel = new(api);
        viewModel.Meetings.Add(proposed);

        await viewModel.AcceptCommand.ExecuteAsync(proposed);

        viewModel.Meetings.Should().ContainSingle().Which.Status.Should().Be(MeetingStatusModel.Accepted);
    }

    private static MatchModel Match() => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Lua", "SRD", DateTimeOffset.UtcNow);
    private static MeetingModel Meeting(MeetingStatusModel status) => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), "Parque", null, status, true, true, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
}
