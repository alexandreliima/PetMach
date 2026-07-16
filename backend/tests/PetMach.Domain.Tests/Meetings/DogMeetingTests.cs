using FluentAssertions;
using PetMach.Domain.Meetings;
using PetMach.Domain.Notifications;

namespace PetMach.Domain.Tests.Meetings;

public sealed class DogMeetingTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void RecipientShouldAcceptAProposal()
    {
        Guid proposer = Guid.NewGuid();
        DogMeeting meeting = new(Guid.NewGuid(), proposer, Now.AddDays(1), "Parque público", null, Now);

        bool accepted = meeting.Accept(Guid.NewGuid(), Now.AddMinutes(1));

        accepted.Should().BeTrue();
        meeting.Status.Should().Be(MeetingStatus.Accepted);
    }

    [Fact]
    public void ProposerShouldNotAcceptOwnProposal()
    {
        Guid proposer = Guid.NewGuid();
        DogMeeting meeting = new(Guid.NewGuid(), proposer, Now.AddDays(1), "Parque público", null, Now);
        meeting.Accept(proposer, Now.AddMinutes(1)).Should().BeFalse();
        meeting.Status.Should().Be(MeetingStatus.Proposed);
    }

    [Fact]
    public void DeclinedMeetingShouldNotBeCancelled()
    {
        DogMeeting meeting = new(Guid.NewGuid(), Guid.NewGuid(), Now.AddDays(1), "Parque público", null, Now);
        meeting.Decline(Guid.NewGuid(), Now.AddMinutes(1));
        meeting.Cancel(Now.AddMinutes(2)).Should().BeFalse();
        meeting.Status.Should().Be(MeetingStatus.Declined);
    }

    [Fact]
    public void MeetingShouldRejectPastSchedule()
    {
        Action create = () => _ = new DogMeeting(Guid.NewGuid(), Guid.NewGuid(), Now.AddMinutes(-1), "Parque", null, Now);
        create.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MeetingNotificationShouldReferenceMeetingWithoutMatchReference()
    {
        Guid meetingId = Guid.NewGuid();

        UserNotification notification = UserNotification.ForMeeting(Guid.NewGuid(), meetingId, "meeting.proposed", "Novo encontro", "Existe uma nova proposta.", Now);

        notification.MeetingId.Should().Be(meetingId);
        notification.MatchId.Should().BeNull();
        notification.Type.Should().Be("meeting.proposed");
    }
}
