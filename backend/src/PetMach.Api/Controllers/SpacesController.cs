using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMach.Application.Partners;
using PetMach.Contracts.Partners;

namespace PetMach.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/spaces")]
public sealed class SpacesController(IPartnerService partners) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyCollection<PartnerSpaceResponse>> List([FromQuery] string? city, [FromQuery] string? state, CancellationToken cancellationToken) =>
        partners.ListSpacesAsync(city, state, cancellationToken);

    [HttpGet("{spaceId:guid}/availability")]
    public Task<IReadOnlyCollection<SpaceAvailabilityResponse>> ListAvailability(Guid spaceId, [FromQuery] DateTimeOffset? fromUtc, [FromQuery] DateTimeOffset? toUtc, CancellationToken cancellationToken) =>
        partners.ListAvailabilityAsync(spaceId, fromUtc, toUtc, cancellationToken);
}
