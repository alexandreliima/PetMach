using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMach.Application.Moderation;
using PetMach.Contracts.Moderation;
using PetMach.Domain.SharedKernel;

namespace PetMach.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/reports")]
public sealed class ReportsController(IReportService reports) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyCollection<ReportResponse>> ListMine(CancellationToken cancellationToken) => reports.ListMineAsync(UserId(), cancellationToken);

    [HttpPost]
    public async Task<IActionResult> Create(CreateReportRequest request, CancellationToken cancellationToken)
    {
        Result<ReportResponse> result = await reports.CreateAsync(UserId(), request, cancellationToken);
        return result.IsSuccess ? Created($"/api/v1/reports/{result.Value.Id}", result.Value) : Error(result.Error);
    }

    [HttpPost("{reportId:guid}/evidence")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> AddEvidence(Guid reportId, IFormFile file, CancellationToken cancellationToken)
    {
        await using Stream stream = file.OpenReadStream();
        Result<ReportEvidenceResponse> result = await reports.AddEvidenceAsync(UserId(), reportId, stream, file.Length, cancellationToken);
        return result.IsSuccess ? Created($"/api/v1/moderation/evidence/{result.Value.Id}", result.Value) : Error(result.Error);
    }

    private ObjectResult Error(DomainError error)
    {
        int status = error.Code.EndsWith("not_found", StringComparison.Ordinal) ? StatusCodes.Status404NotFound :
            error.Code.EndsWith("conflict", StringComparison.Ordinal) ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest;
        return StatusCode(status, new ProblemDetails { Status = status, Title = error.Description });
    }
    private Guid UserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out Guid id) ? id : Guid.Empty;
}
