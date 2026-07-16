using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMach.Application.Matches;
using PetMach.Contracts.Matches;
using PetMach.Domain.SharedKernel;

namespace PetMach.Api.Controllers;

[ApiController]
[Authorize(Policy = "TutorAccess")]
[Route("api/v1/matches")]
public sealed class MatchesController(IMatchService matches) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyCollection<MatchResponse>> List(CancellationToken cancellationToken) => matches.ListAsync(UserId(), cancellationToken);

    [HttpDelete("{matchId:guid}")]
    public async Task<IActionResult> End(Guid matchId, CancellationToken cancellationToken)
    {
        Result result = await matches.EndAsync(UserId(), matchId, cancellationToken);
        if (result.IsSuccess) return NoContent();
        ProblemDetails problem = new() { Status = StatusCodes.Status404NotFound, Title = result.Error.Description, Type = $"https://petmach.local/problems/{result.Error.Code}" };
        problem.Extensions["code"] = result.Error.Code;
        return NotFound(problem);
    }

    private Guid UserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out Guid id) ? id : Guid.Empty;
}
