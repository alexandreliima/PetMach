using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMach.Application.Partners;
using PetMach.Contracts.Partners;
using PetMach.Domain.SharedKernel;

namespace PetMach.Api.Controllers;

[ApiController]
[Route("api/v1/partners")]
public sealed class PartnersController(IPartnerService partners) : ControllerBase
{
    [HttpGet("me")]
    [Authorize(Policy = "PartnerAccess")]
    public async Task<IActionResult> GetManaged(CancellationToken cancellationToken)
    {
        Result<PartnerManagementResponse> result = await partners.GetManagedAsync(UserId(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Error(result.Error);
    }

    [HttpGet("me/spaces")]
    [Authorize(Policy = "PartnerAccess")]
    public Task<IReadOnlyCollection<PartnerSpaceResponse>> ListManagedSpaces(CancellationToken cancellationToken) =>
        partners.ListManagedSpacesAsync(UserId(), cancellationToken);

    [HttpPost]
    [Authorize(Policy = "PartnerAccess")]
    public async Task<IActionResult> Create(CreatePartnerRequest request, CancellationToken cancellationToken)
    {
        Result<PartnerManagementResponse> result = await partners.CreateAsync(UserId(), request, cancellationToken);
        return result.IsSuccess ? Created($"/api/v1/partners/{result.Value.Id}", result.Value) : Error(result.Error);
    }

    [HttpPost("{establishmentId:guid}/spaces")]
    [Authorize(Policy = "PartnerAccess")]
    public async Task<IActionResult> CreateSpace(Guid establishmentId, CreateSpaceRequest request, CancellationToken cancellationToken)
    {
        Result<PartnerSpaceResponse> result = await partners.CreateSpaceAsync(UserId(), establishmentId, request, cancellationToken);
        return result.IsSuccess ? Created($"/api/v1/spaces/{result.Value.Id}", result.Value) : Error(result.Error);
    }

    [HttpPost("spaces/{spaceId:guid}/availability")]
    [Authorize(Policy = "PartnerAccess")]
    public async Task<IActionResult> CreateAvailability(Guid spaceId, CreateAvailabilityRequest request, CancellationToken cancellationToken)
    {
        Result<SpaceAvailabilityResponse> result = await partners.CreateAvailabilityAsync(UserId(), spaceId, request, cancellationToken);
        return result.IsSuccess ? Created($"/api/v1/spaces/{spaceId}/availability/{result.Value.Id}", result.Value) : Error(result.Error);
    }

    private ObjectResult Error(DomainError error)
    {
        int status = error.Code.EndsWith("not_found", StringComparison.Ordinal) ? StatusCodes.Status404NotFound :
            error.Code.EndsWith("conflict", StringComparison.Ordinal) ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest;
        return StatusCode(status, new ProblemDetails { Status = status, Title = error.Description });
    }

    private Guid UserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out Guid id) ? id : Guid.Empty;
}
