using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMach.Application.Notifications;
using PetMach.Contracts.Notifications;
using PetMach.Domain.SharedKernel;

namespace PetMach.Api.Controllers;

[ApiController]
[Authorize(Policy = "TutorAccess")]
[Route("api/v1/notifications")]
public sealed class NotificationsController(INotificationService notifications) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyCollection<NotificationResponse>> List(CancellationToken cancellationToken) =>
        notifications.ListAsync(UserId(), cancellationToken);

    [HttpPut("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId, CancellationToken cancellationToken)
    {
        Result result = await notifications.MarkAsReadAsync(UserId(), notificationId, cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound();
    }

    private Guid UserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out Guid id) ? id : Guid.Empty;
}
