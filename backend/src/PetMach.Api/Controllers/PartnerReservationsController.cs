using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMach.Application.Reservations;
using PetMach.Contracts.Reservations;
using PetMach.Domain.SharedKernel;

namespace PetMach.Api.Controllers;

[ApiController]
[Authorize(Policy = "PartnerAccess")]
[Route("api/v1/partners/reservations")]
public sealed class PartnerReservationsController(IReservationService reservations) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyCollection<ReservationResponse>> List(CancellationToken cancellationToken) => reservations.ListForPartnerAsync(UserId(), cancellationToken);

    [HttpPut("{reservationId:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid reservationId, CancellationToken cancellationToken)
    {
        return await Transition(reservations.ConfirmAsync(UserId(), reservationId, cancellationToken));
    }

    [HttpPut("{reservationId:guid}/cancel")]
    public Task<IActionResult> Cancel(Guid reservationId, CancellationToken cancellationToken) => Transition(reservations.CancelForPartnerAsync(UserId(), reservationId, cancellationToken));

    [HttpPut("{reservationId:guid}/complete")]
    public Task<IActionResult> Complete(Guid reservationId, CompleteReservationRequest request, CancellationToken cancellationToken) => Transition(reservations.CompleteAsync(UserId(), reservationId, request.PaymentReceivedOnSite, cancellationToken));

    [HttpPut("{reservationId:guid}/no-show")]
    public Task<IActionResult> MarkNoShow(Guid reservationId, CancellationToken cancellationToken) => Transition(reservations.MarkNoShowAsync(UserId(), reservationId, cancellationToken));

    [HttpGet("{reservationId:guid}/history")]
    public async Task<IActionResult> History(Guid reservationId, CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<ReservationHistoryResponse>> result = await reservations.HistoryAsync(UserId(), true, reservationId, cancellationToken);
        if (result.IsSuccess) return Ok(result.Value);
        return NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = result.Error.Description });
    }

    private static async Task<IActionResult> Transition(Task<Result<ReservationResponse>> operation)
    {
        Result<ReservationResponse> result = await operation;
        if (result.IsSuccess) return new OkObjectResult(result.Value);
        int status = result.Error.Code.EndsWith("not_found", StringComparison.Ordinal) ? StatusCodes.Status404NotFound : StatusCodes.Status409Conflict;
        return new ObjectResult(new ProblemDetails { Status = status, Title = result.Error.Description }) { StatusCode = status };
    }

    private Guid UserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out Guid id) ? id : Guid.Empty;
}
