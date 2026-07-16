using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMach.Application.Discovery;
using PetMach.Contracts.Discovery;
using PetMach.Domain.SharedKernel;

namespace PetMach.Api.Controllers;

[ApiController]
[Authorize(Policy = "TutorAccess")]
[Route("api/v1/discovery")]
public sealed class DiscoveryController(IDiscoveryService discovery) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Discover([FromQuery] DiscoveryFilterRequest request, CancellationToken cancellationToken)
    {
        Result<DiscoveryPageResponse> result = await discovery.DiscoverAsync(UserId(), request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Error(result.Error);
    }

    [HttpGet("dogs/{dogId:guid}/photo")]
    public async Task<IActionResult> Photo(Guid dogId, CancellationToken cancellationToken)
    {
        Result<DiscoveryImage> result = await discovery.GetPrimaryPhotoAsync(UserId(), dogId, cancellationToken);
        return result.IsSuccess ? File(result.Value.Content, result.Value.ContentType) : Error(result.Error);
    }

    private Guid UserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out Guid id) ? id : Guid.Empty;

    private ObjectResult Error(DomainError error)
    {
        int status = error.Code.EndsWith("not_found", StringComparison.Ordinal) ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
        ProblemDetails problem = new() { Status = status, Title = error.Description, Type = $"https://petmach.local/problems/{error.Code}" };
        problem.Extensions["code"] = error.Code;
        return StatusCode(status, problem);
    }
}
