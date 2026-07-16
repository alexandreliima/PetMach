namespace PetMach.Contracts.Meetings;

public enum MeetingStatusContract { Proposed, Accepted, Declined, Cancelled }
public sealed record CreateMeetingRequest(Guid MatchId, DateTimeOffset ScheduledAtUtc, string PlaceName, string? Notes);
public sealed record MeetingResponse(Guid Id, Guid MatchId, Guid ProposedByUserId, DateTimeOffset ScheduledAtUtc, string PlaceName, string? Notes, MeetingStatusContract Status, bool CanRespond, bool CanCancel, DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc);
