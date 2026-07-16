using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMach.Application.Identity;
using PetMach.Contracts.Identity;
using PetMach.Domain.SharedKernel;

namespace PetMach.Api.Controllers;

[ApiController]
[Authorize(Policy = "AdministrationAccess")]
[Route("api/v1/administration/users")]
public sealed class AdministrationIdentityController(IIdentityService identity) : ControllerBase
{
    [HttpPatch("{userId:guid}/suspension")]
    public async Task<IActionResult> SetSuspension(Guid userId, SetAccountSuspensionRequest request, CancellationToken cancellationToken)
    {
        string? value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        Guid actorUserId = Guid.TryParse(value, out Guid parsed) ? parsed : Guid.Empty;
        Result result = await identity.SetSuspensionAsync(actorUserId, userId, request, cancellationToken);
        if (result.IsSuccess) return NoContent();
        ProblemDetails problem = new()
        {
            Status = StatusCodes.Status404NotFound,
            Title = result.Error.Description,
            Type = $"https://petmach.local/problems/{result.Error.Code}",
        };
        problem.Extensions["code"] = result.Error.Code;
        return NotFound(problem);
    }
}
