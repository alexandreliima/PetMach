using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMach.Application.Reservations;
using PetMach.Contracts.Reservations;
using PetMach.Domain.SharedKernel;

namespace PetMach.Api.Controllers;

[ApiController]
[Authorize(Policy = "TutorAccess")]
[Route("api/v1/reservations")]
public sealed class ReservationsController(IReservationService reservations) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyCollection<ReservationResponse>> List(CancellationToken cancellationToken) => reservations.ListForTutorAsync(UserId(), cancellationToken);

    [HttpPost]
    public async Task<IActionResult> Create(CreateReservationRequest request, CancellationToken cancellationToken)
    {
        Result<ReservationResponse> result = await reservations.CreateAsync(UserId(), request, cancellationToken);
        return result.IsSuccess ? Created($"/api/v1/reservations/{result.Value.Id}", result.Value) : Error(result.Error);
    }

    [HttpPut("{reservationId:guid}/cancel")]
    public Task<IActionResult> Cancel(Guid reservationId, CancellationToken cancellationToken) =>
        Transition(reservations.CancelForTutorAsync(UserId(), reservationId, cancellationToken));

    [HttpGet("{reservationId:guid}/history")]
    public async Task<IActionResult> History(Guid reservationId, CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<ReservationHistoryResponse>> result = await reservations.HistoryAsync(UserId(), false, reservationId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Error(result.Error);
    }

    private async Task<IActionResult> Transition(Task<Result<ReservationResponse>> operation)
    {
        Result<ReservationResponse> result = await operation;
        return result.IsSuccess ? Ok(result.Value) : Error(result.Error);
    }

    private ObjectResult Error(DomainError error)
    {
        int status = error.Code.EndsWith("not_found", StringComparison.Ordinal) ? StatusCodes.Status404NotFound :
            error.Code.EndsWith("conflict", StringComparison.Ordinal) ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest;
        return StatusCode(status, new ProblemDetails { Status = status, Title = error.Description });
    }

    private Guid UserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out Guid id) ? id : Guid.Empty;
}
