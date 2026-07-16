namespace PetMach.Domain.Meetings;

public enum MeetingStatus { Proposed, Accepted, Declined, Cancelled }

public sealed class DogMeeting
{
    private DogMeeting() { }

    public DogMeeting(Guid matchId, Guid proposedByUserId, DateTimeOffset scheduledAtUtc, string placeName, string? notes, DateTimeOffset now)
    {
        if (matchId == Guid.Empty || proposedByUserId == Guid.Empty || scheduledAtUtc <= now || string.IsNullOrWhiteSpace(placeName) || placeName.Trim().Length > 160 || notes?.Trim().Length > 1000)
            throw new ArgumentException("Proposta de encontro inválida.");
        Id = Guid.NewGuid();
        MatchId = matchId;
        ProposedByUserId = proposedByUserId;
        ScheduledAtUtc = scheduledAtUtc.ToUniversalTime();
        PlaceName = placeName.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        Status = MeetingStatus.Proposed;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public Guid MatchId { get; private set; }
    public Guid ProposedByUserId { get; private set; }
    public DateTimeOffset ScheduledAtUtc { get; private set; }
    public string PlaceName { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public MeetingStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public bool Accept(Guid actorUserId, DateTimeOffset now)
    {
        if (Status != MeetingStatus.Proposed || actorUserId == ProposedByUserId) return false;
        Status = MeetingStatus.Accepted;
        UpdatedAtUtc = now;
        return true;
    }

    public bool Decline(Guid actorUserId, DateTimeOffset now)
    {
        if (Status != MeetingStatus.Proposed || actorUserId == ProposedByUserId) return false;
        Status = MeetingStatus.Declined;
        UpdatedAtUtc = now;
        return true;
    }

    public bool Cancel(DateTimeOffset now)
    {
        if (Status is not (MeetingStatus.Proposed or MeetingStatus.Accepted)) return false;
        Status = MeetingStatus.Cancelled;
        UpdatedAtUtc = now;
        return true;
    }
}
