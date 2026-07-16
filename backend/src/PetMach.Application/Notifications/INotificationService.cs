using PetMach.Contracts.Notifications;
using PetMach.Domain.SharedKernel;

namespace PetMach.Application.Notifications;

public interface INotificationService
{
    Task<IReadOnlyCollection<NotificationResponse>> ListAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken);
}
