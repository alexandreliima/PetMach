namespace PetMach.Contracts.Chat;

public sealed record ConversationResponse(Guid Id, Guid MatchId, string OtherDogName, int UnreadCount, DateTimeOffset CreatedAtUtc);
public sealed record SendMessageRequest(string Content);
public sealed record MarkConversationReadRequest(Guid MessageId);
public sealed record ConversationReadResponse(Guid ConversationId, Guid UserId, Guid MessageId, DateTimeOffset ReadAtUtc);
public sealed record ChatMessageResponse(Guid Id, Guid ConversationId, Guid SenderUserId, string Content, DateTimeOffset SentAtUtc);
public sealed record ChatMessagePageResponse(IReadOnlyCollection<ChatMessageResponse> Items, int Page, int PageSize, bool HasMore);
