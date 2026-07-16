namespace PetMach.Domain.Notifications;

public sealed class UserNotification
{
    private UserNotification() { }

    public UserNotification(Guid recipientUserId, Guid matchId, string title, string message, DateTimeOffset now)
    {
        if (recipientUserId == Guid.Empty || matchId == Guid.Empty || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Notificação inválida.");

        Id = Guid.NewGuid();
        RecipientUserId = recipientUserId;
        MatchId = matchId;
        Type = "match.created";
        Title = title.Trim();
        Message = message.Trim();
        CreatedAtUtc = now;
    }

    public static UserNotification ForMeeting(Guid recipientUserId, Guid meetingId, string type, string title, string message, DateTimeOffset now)
    {
        if (recipientUserId == Guid.Empty || meetingId == Guid.Empty || string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Notificação inválida.");
        return new UserNotification
        {
            Id = Guid.NewGuid(),
            RecipientUserId = recipientUserId,
            MeetingId = meetingId,
            Type = type.Trim(),
            Title = title.Trim(),
            Message = message.Trim(),
            CreatedAtUtc = now,
        };
    }

    public Guid Id { get; private set; }
    public Guid RecipientUserId { get; private set; }
    public Guid? MatchId { get; private set; }
    public Guid? MeetingId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? ReadAtUtc { get; private set; }

    public void MarkAsRead(DateTimeOffset now) => ReadAtUtc ??= now;
}
