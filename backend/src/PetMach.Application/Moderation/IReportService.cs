using PetMach.Contracts.Moderation;
using PetMach.Domain.SharedKernel;

namespace PetMach.Application.Moderation;

public sealed record ProtectedEvidenceFile(Stream Content, string ContentType);

public interface IReportService
{
    Task<Result<ReportResponse>> CreateAsync(Guid reporterUserId, CreateReportRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ReportResponse>> ListMineAsync(Guid reporterUserId, CancellationToken cancellationToken);
    Task<Result<ReportEvidenceResponse>> AddEvidenceAsync(Guid reporterUserId, Guid reportId, Stream content, long length, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ReportResponse>> QueueAsync(CancellationToken cancellationToken);
    Task<Result<ReportResponse>> TransitionAsync(Guid moderatorUserId, Guid reportId, string transition, CancellationToken cancellationToken);
    Task<Result<ProtectedEvidenceFile>> GetEvidenceAsync(Guid evidenceId, CancellationToken cancellationToken);
}
