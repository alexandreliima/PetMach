namespace PetMach.Domain.Chat;

public sealed class ChatMessage
{
    private ChatMessage() { }

    public ChatMessage(Guid conversationId, Guid senderUserId, string content, DateTimeOffset now)
    {
        if (conversationId == Guid.Empty || senderUserId == Guid.Empty || string.IsNullOrWhiteSpace(content) || content.Trim().Length > 2000)
            throw new ArgumentException("Mensagem inválida.");
        Id = Guid.NewGuid();
        ConversationId = conversationId;
        SenderUserId = senderUserId;
        Content = content.Trim();
        SentAtUtc = now;
    }

    public Guid Id { get; private set; }
    public Guid ConversationId { get; private set; }
    public Guid SenderUserId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTimeOffset SentAtUtc { get; private set; }
}
