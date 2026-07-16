using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMach.Application.Tutors;
using PetMach.Contracts.Tutors;
using PetMach.Domain.SharedKernel;

namespace PetMach.Api.Controllers;

[ApiController]
[Authorize(Policy = "TutorAccess")]
[Route("api/v1/tutors/me")]
public sealed class TutorsController(ITutorProfileService profiles) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<TutorProfileResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        Result<TutorProfileResponse> result = await profiles.GetAsync(CurrentUserId(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ProblemResult(result, StatusCodes.Status404NotFound);
    }

    [HttpPut]
    [ProducesResponseType<TutorProfileResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Upsert(UpsertTutorProfileRequest request, CancellationToken cancellationToken)
    {
        Result<TutorProfileResponse> result = await profiles.UpsertAsync(CurrentUserId(), request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ProblemResult(result, StatusCodes.Status400BadRequest);
    }

    private Guid CurrentUserId()
    {
        string? value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(value, out Guid userId) ? userId : Guid.Empty;
    }

    private ObjectResult ProblemResult(Result<TutorProfileResponse> result, int status)
    {
        ProblemDetails problem = new() { Status = status, Title = result.Error.Description, Type = $"https://petmach.local/problems/{result.Error.Code}" };
        problem.Extensions["code"] = result.Error.Code;
        problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
        return StatusCode(status, problem);
    }
}
