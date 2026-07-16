using PetMach.Contracts.Chat;
using PetMach.Domain.SharedKernel;

namespace PetMach.Application.Chat;

public interface IChatService
{
    Task<IReadOnlyCollection<ConversationResponse>> ListAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result<ChatMessagePageResponse>> HistoryAsync(Guid userId, Guid conversationId, int page, int pageSize, CancellationToken cancellationToken);
    Task<Result<ChatMessageResponse>> SendAsync(Guid userId, Guid conversationId, SendMessageRequest request, CancellationToken cancellationToken);
    Task<Result<ConversationReadResponse>> MarkReadAsync(Guid userId, Guid conversationId, MarkConversationReadRequest request, CancellationToken cancellationToken);
    Task<bool> CanAccessAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken);
}
