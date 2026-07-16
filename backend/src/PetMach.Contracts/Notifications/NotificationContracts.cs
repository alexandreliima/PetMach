namespace PetMach.Contracts.Notifications;

public sealed record NotificationResponse(
    Guid Id,
    Guid? MatchId,
    Guid? MeetingId,
    string Type,
    string Title,
    string Message,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ReadAtUtc);
