using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMach.Application.Moderation;
using PetMach.Domain.SharedKernel;

namespace PetMach.Api.Controllers;

[ApiController]
[Authorize(Policy = "TutorAccess")]
[Route("api/v1/dogs/{targetDogId:guid}/block-owner")]
public sealed class BlocksController(IBlockService blocks) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Block(Guid targetDogId, CancellationToken cancellationToken)
    {
        Result result = await blocks.BlockDogOwnerAsync(UserId(), targetDogId, cancellationToken);
        if (result.IsSuccess) return NoContent();
        ProblemDetails problem = new() { Status = StatusCodes.Status400BadRequest, Title = result.Error.Description, Type = $"https://petmach.local/problems/{result.Error.Code}" };
        problem.Extensions["code"] = result.Error.Code;
        return BadRequest(problem);
    }

    private Guid UserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out Guid id) ? id : Guid.Empty;
}
