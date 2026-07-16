using Microsoft.EntityFrameworkCore;
using PetMach.Application.Chat;
using PetMach.Contracts.Chat;
using PetMach.Domain.Chat;
using PetMach.Domain.SharedKernel;
using PetMach.Infrastructure.Persistence;

namespace PetMach.Infrastructure.Chat;

internal sealed class ChatService(PetMachDbContext dbContext, TimeProvider timeProvider) : IChatService
{
    private static readonly DomainError NotFound = new("chat.not_found", "Conversa não encontrada.");
    private static readonly DomainError InvalidMessage = new("chat.invalid_message", "A mensagem deve ter entre 1 e 2000 caracteres.");

    public async Task<IReadOnlyCollection<ConversationResponse>> ListAsync(Guid userId, CancellationToken cancellationToken) =>
        await (
            from conversation in dbContext.Conversations.AsNoTracking()
            join match in dbContext.DogMatches.AsNoTracking() on conversation.MatchId equals match.Id
            join dogA in dbContext.Dogs.AsNoTracking() on match.DogAId equals dogA.Id
            join dogB in dbContext.Dogs.AsNoTracking() on match.DogBId equals dogB.Id
            where match.EndedAtUtc == null &&
                (dogA.OwnerUserId == userId || dogB.OwnerUserId == userId) &&
                !dbContext.BlockedUsers.Any(block =>
                    (block.UserId == dogA.OwnerUserId && block.BlockedUserId == dogB.OwnerUserId) ||
                    (block.UserId == dogB.OwnerUserId && block.BlockedUserId == dogA.OwnerUserId))
            orderby conversation.CreatedAtUtc descending
            select new ConversationResponse(
                conversation.Id,
                match.Id,
                dogA.OwnerUserId == userId ? dogB.Name : dogA.Name,
                dbContext.ChatMessages.Count(message => message.ConversationId == conversation.Id && message.SenderUserId != userId &&
                    message.SentAtUtc > (dbContext.ConversationReadStates
                        .Where(state => state.ConversationId == conversation.Id && state.UserId == userId)
                        .Select(state => (DateTimeOffset?)state.LastReadMessageAtUtc)
                        .FirstOrDefault() ?? DateTimeOffset.MinValue)),
                conversation.CreatedAtUtc))
            .ToArrayAsync(cancellationToken);

    public async Task<Result<ChatMessagePageResponse>> HistoryAsync(Guid userId, Guid conversationId, int page, int pageSize, CancellationToken cancellationToken)
    {
        if (page < 1 || pageSize is < 1 or > 50 || !await CanAccessAsync(userId, conversationId, cancellationToken))
            return Result.Failure<ChatMessagePageResponse>(NotFound);
        ChatMessageResponse[] messages = await dbContext.ChatMessages.AsNoTracking()
            .Where(x => x.ConversationId == conversationId)
            .OrderByDescending(x => x.SentAtUtc).ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize).Take(pageSize + 1)
            .Select(x => new ChatMessageResponse(x.Id, x.ConversationId, x.SenderUserId, x.Content, x.SentAtUtc))
            .ToArrayAsync(cancellationToken);
        return Result.Success(new ChatMessagePageResponse(messages.Take(pageSize).ToArray(), page, pageSize, messages.Length > pageSize));
    }

    public async Task<Result<ChatMessageResponse>> SendAsync(Guid userId, Guid conversationId, SendMessageRequest request, CancellationToken cancellationToken)
    {
        if (!await CanAccessAsync(userId, conversationId, cancellationToken)) return Result.Failure<ChatMessageResponse>(NotFound);
        if (string.IsNullOrWhiteSpace(request.Content) || request.Content.Trim().Length > 2000)
            return Result.Failure<ChatMessageResponse>(InvalidMessage);
        ChatMessage message = new(conversationId, userId, request.Content, timeProvider.GetUtcNow());
        dbContext.ChatMessages.Add(message);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(new ChatMessageResponse(message.Id, message.ConversationId, message.SenderUserId, message.Content, message.SentAtUtc));
    }

    public async Task<Result<ConversationReadResponse>> MarkReadAsync(Guid userId, Guid conversationId, MarkConversationReadRequest request, CancellationToken cancellationToken)
    {
        if (!await CanAccessAsync(userId, conversationId, cancellationToken))
            return Result.Failure<ConversationReadResponse>(NotFound);
        ChatMessage? message = await dbContext.ChatMessages.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.MessageId && x.ConversationId == conversationId, cancellationToken);
        if (message is null) return Result.Failure<ConversationReadResponse>(NotFound);
        DateTimeOffset now = timeProvider.GetUtcNow();
        ConversationReadState? state = await dbContext.ConversationReadStates
            .SingleOrDefaultAsync(x => x.ConversationId == conversationId && x.UserId == userId, cancellationToken);
        if (state is null)
        {
            state = new ConversationReadState(conversationId, userId, message.Id, message.SentAtUtc, now);
            dbContext.ConversationReadStates.Add(state);
        }
        else
        {
            state.Advance(message.Id, message.SentAtUtc, now);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(new ConversationReadResponse(conversationId, userId, state.LastReadMessageId, state.UpdatedAtUtc));
    }

    public Task<bool> CanAccessAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken) =>
        (from conversation in dbContext.Conversations.AsNoTracking()
         join match in dbContext.DogMatches.AsNoTracking() on conversation.MatchId equals match.Id
         join dogA in dbContext.Dogs.AsNoTracking() on match.DogAId equals dogA.Id
         join dogB in dbContext.Dogs.AsNoTracking() on match.DogBId equals dogB.Id
         where conversation.Id == conversationId && match.EndedAtUtc == null &&
             (dogA.OwnerUserId == userId || dogB.OwnerUserId == userId) &&
             !dbContext.BlockedUsers.Any(block =>
                 (block.UserId == dogA.OwnerUserId && block.BlockedUserId == dogB.OwnerUserId) ||
                 (block.UserId == dogB.OwnerUserId && block.BlockedUserId == dogA.OwnerUserId))
         select conversation.Id).AnyAsync(cancellationToken);
}
