namespace PetMach.Domain.Chat;

public sealed class ConversationReadState
{
    private ConversationReadState() { }

    public ConversationReadState(Guid conversationId, Guid userId, Guid lastReadMessageId, DateTimeOffset lastReadMessageAtUtc, DateTimeOffset now)
    {
        if (conversationId == Guid.Empty || userId == Guid.Empty || lastReadMessageId == Guid.Empty)
            throw new ArgumentException("Estado de leitura inválido.");
        Id = Guid.NewGuid();
        ConversationId = conversationId;
        UserId = userId;
        LastReadMessageId = lastReadMessageId;
        LastReadMessageAtUtc = lastReadMessageAtUtc;
        UpdatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public Guid ConversationId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid LastReadMessageId { get; private set; }
    public DateTimeOffset LastReadMessageAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public void Advance(Guid messageId, DateTimeOffset messageAtUtc, DateTimeOffset now)
    {
        if (messageId == Guid.Empty || messageAtUtc <= LastReadMessageAtUtc) return;
        LastReadMessageId = messageId;
        LastReadMessageAtUtc = messageAtUtc;
        UpdatedAtUtc = now;
    }
}
