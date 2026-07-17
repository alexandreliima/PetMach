using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMach.Application.Moderation;
using PetMach.Contracts.Moderation;
using PetMach.Domain.SharedKernel;

namespace PetMach.Api.Controllers;

[ApiController]
[Authorize(Policy = "AdministrationAccess")]
[Route("api/v1/moderation")]
public sealed class ModerationQueueController(IReportService reports) : ControllerBase
{
    [HttpGet("reports")]
    public Task<IReadOnlyCollection<ReportResponse>> Queue(CancellationToken cancellationToken) => reports.QueueAsync(cancellationToken);

    [HttpPut("reports/{reportId:guid}/{transition}")]
    public async Task<IActionResult> Transition(Guid reportId, string transition, CancellationToken cancellationToken)
    {
        Result<ReportResponse> result = await reports.TransitionAsync(UserId(), reportId, transition, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.Error.Code.EndsWith("not_found", StringComparison.Ordinal) ? 404 : 409, new ProblemDetails { Title = result.Error.Description });
    }

    [HttpGet("evidence/{evidenceId:guid}")]
    public async Task<IActionResult> Evidence(Guid evidenceId, CancellationToken cancellationToken)
    {
        Result<ProtectedEvidenceFile> result = await reports.GetEvidenceAsync(evidenceId, cancellationToken);
        return result.IsSuccess ? File(result.Value.Content, result.Value.ContentType) : NotFound();
    }

    [HttpGet("reports/{reportId:guid}/evidence")]
    public async Task<IActionResult> ListEvidence(Guid reportId, CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<ReportEvidenceResponse>> result = await reports.ListEvidenceAsync(reportId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }

    [HttpPost("reports/{reportId:guid}/actions")]
    public async Task<IActionResult> ApplyAction(Guid reportId, ApplyModerationActionRequest request, CancellationToken cancellationToken)
    {
        Result<ModerationActionResponse> result = await reports.ApplyActionAsync(UserId(), reportId, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.Error.Code.EndsWith("not_found", StringComparison.Ordinal) ? 404 : 409, new ProblemDetails { Title = result.Error.Description });
    }
    private Guid UserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out Guid id) ? id : Guid.Empty;
}
