using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMach.Application.Adoption;
using PetMach.Contracts.Adoption;
using PetMach.Domain.SharedKernel;

namespace PetMach.Api.Controllers;

[ApiController]
[Authorize(Policy = "TutorAccess")]
[Route("api/v1/adoption")]
public sealed class AdoptionController(IAdoptionService adoption) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyCollection<AdoptionProfileResponse>> List(CancellationToken cancellationToken) => adoption.ListAsync(UserId(), cancellationToken);

    [HttpPost]
    public async Task<IActionResult> Create(CreateAdoptionProfileRequest request, CancellationToken cancellationToken)
    {
        Result<AdoptionProfileResponse> result = await adoption.CreateAsync(UserId(), request, cancellationToken);
        return result.IsSuccess ? Created($"/api/v1/adoption/{result.Value.Id}", result.Value) : Error(result.Error);
    }

    [HttpPut("{profileId:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid profileId, CancellationToken cancellationToken)
    {
        Result result = await adoption.SuspendAsync(UserId(), profileId, cancellationToken);
        return result.IsSuccess ? NoContent() : Error(result.Error);
    }

    [HttpPost("{profileId:guid}/applications")]
    public async Task<IActionResult> Apply(Guid profileId, CreateAdoptionApplicationRequest request, CancellationToken cancellationToken)
    {
        Result<AdoptionApplicationResponse> result = await adoption.ApplyAsync(UserId(), profileId, request, cancellationToken);
        return result.IsSuccess ? Created($"/api/v1/adoption/applications/{result.Value.Id}", result.Value) : Error(result.Error);
    }

    [HttpGet("applications")]
    public Task<IReadOnlyCollection<AdoptionApplicationResponse>> ListMyApplications(CancellationToken cancellationToken) =>
        adoption.ListMyApplicationsAsync(UserId(), cancellationToken);

    [HttpGet("{profileId:guid}/applications")]
    public async Task<IActionResult> ListProfileApplications(Guid profileId, CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<AdoptionApplicationResponse>> result = await adoption.ListProfileApplicationsAsync(UserId(), profileId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Error(result.Error);
    }

    [HttpPut("applications/{applicationId:guid}/{transition}")]
    public async Task<IActionResult> TransitionApplication(Guid applicationId, string transition, CancellationToken cancellationToken)
    {
        Result<AdoptionApplicationResponse> result = await adoption.TransitionApplicationAsync(UserId(), applicationId, transition, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Error(result.Error);
    }

    [HttpGet("applications/{applicationId:guid}/history")]
    public async Task<IActionResult> ApplicationHistory(Guid applicationId, CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<AdoptionApplicationHistoryResponse>> result = await adoption.ApplicationHistoryAsync(UserId(), applicationId, cancellationToken);
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
