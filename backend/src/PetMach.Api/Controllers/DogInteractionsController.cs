using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMach.Application.Discovery;
using PetMach.Contracts.Discovery;
using PetMach.Domain.SharedKernel;

namespace PetMach.Api.Controllers;

[ApiController]
[Authorize(Policy = "TutorAccess")]
[Route("api/v1/dogs/{targetDogId:guid}")]
public sealed class DogInteractionsController(IDiscoveryService discovery) : ControllerBase
{
    [HttpPost("likes")]
    public async Task<IActionResult> Like(Guid targetDogId, LikeDogRequest request, CancellationToken cancellationToken)
    {
        Result<LikeDogResponse> result = await discovery.LikeAsync(UserId(), targetDogId, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Error(result.Error);
    }

    [HttpPost("passes")]
    public async Task<IActionResult> Pass(Guid targetDogId, PassDogRequest request, CancellationToken cancellationToken)
    {
        Result result = await discovery.PassAsync(UserId(), targetDogId, request, cancellationToken);
        return result.IsSuccess ? NoContent() : Error(result.Error);
    }

    private Guid UserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out Guid id) ? id : Guid.Empty;

    private BadRequestObjectResult Error(DomainError error)
    {
        ProblemDetails problem = new() { Status = StatusCodes.Status400BadRequest, Title = error.Description, Type = $"https://petmach.local/problems/{error.Code}" };
        problem.Extensions["code"] = error.Code;
        return BadRequest(problem);
    }
}
