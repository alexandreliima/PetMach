using Microsoft.EntityFrameworkCore;
using PetMach.Application.Notifications;
using PetMach.Contracts.Notifications;
using PetMach.Domain.Notifications;
using PetMach.Domain.SharedKernel;
using PetMach.Infrastructure.Persistence;

namespace PetMach.Infrastructure.Notifications;

internal sealed class NotificationService(PetMachDbContext dbContext, TimeProvider timeProvider) : INotificationService
{
    private static readonly DomainError NotFound = new("notifications.not_found", "Notificação não encontrada.");

    public async Task<IReadOnlyCollection<NotificationResponse>> ListAsync(Guid userId, CancellationToken cancellationToken) =>
        await dbContext.UserNotifications.AsNoTracking()
            .Where(x => x.RecipientUserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(100)
            .Select(x => new NotificationResponse(x.Id, x.MatchId, x.MeetingId, x.Type, x.Title, x.Message, x.CreatedAtUtc, x.ReadAtUtc))
            .ToArrayAsync(cancellationToken);

    public async Task<Result> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken)
    {
        UserNotification? notification = await dbContext.UserNotifications
            .SingleOrDefaultAsync(x => x.Id == notificationId && x.RecipientUserId == userId, cancellationToken);
        if (notification is null) return Result.Failure(NotFound);
        notification.MarkAsRead(timeProvider.GetUtcNow());
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
