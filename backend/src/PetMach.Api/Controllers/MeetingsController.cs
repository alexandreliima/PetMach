using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMach.Application.Meetings;
using PetMach.Contracts.Meetings;
using PetMach.Domain.SharedKernel;

namespace PetMach.Api.Controllers;

[ApiController]
[Authorize(Policy = "TutorAccess")]
[Route("api/v1/meetings")]
public sealed class MeetingsController(IMeetingService meetings) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyCollection<MeetingResponse>> List(CancellationToken cancellationToken) => meetings.ListAsync(UserId(), cancellationToken);

    [HttpPost]
    public async Task<IActionResult> Create(CreateMeetingRequest request, CancellationToken cancellationToken)
    {
        Result<MeetingResponse> result = await meetings.CreateAsync(UserId(), request, cancellationToken);
        return result.IsSuccess ? Created($"/api/v1/meetings/{result.Value.Id}", result.Value) : Error(result.Error);
    }

    [HttpPut("{meetingId:guid}/accept")]
    public Task<IActionResult> Accept(Guid meetingId, CancellationToken cancellationToken) => Transition(meetings.AcceptAsync(UserId(), meetingId, cancellationToken));

    [HttpPut("{meetingId:guid}/decline")]
    public Task<IActionResult> Decline(Guid meetingId, CancellationToken cancellationToken) => Transition(meetings.DeclineAsync(UserId(), meetingId, cancellationToken));

    [HttpPut("{meetingId:guid}/cancel")]
    public Task<IActionResult> Cancel(Guid meetingId, CancellationToken cancellationToken) => Transition(meetings.CancelAsync(UserId(), meetingId, cancellationToken));

    private async Task<IActionResult> Transition(Task<Result<MeetingResponse>> operation)
    {
        Result<MeetingResponse> result = await operation;
        return result.IsSuccess ? Ok(result.Value) : Error(result.Error);
    }

    private ObjectResult Error(DomainError error)
    {
        int status = error.Code.EndsWith("not_found", StringComparison.Ordinal) ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
        return StatusCode(status, new ProblemDetails { Status = status, Title = error.Description });
    }

    private Guid UserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out Guid id) ? id : Guid.Empty;
}
