using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using PetMach.Application.Moderation;
using PetMach.Contracts.Moderation;
using PetMach.Domain.Moderation;
using PetMach.Domain.SharedKernel;
using PetMach.Infrastructure.Persistence;

namespace PetMach.Infrastructure.Moderation;

internal sealed class ReportService(PetMachDbContext dbContext, IWebHostEnvironment environment, TimeProvider timeProvider) : IReportService
{
    private const long MaximumEvidenceLength = 5 * 1024 * 1024;
    private static readonly DomainError Invalid = new("reports.invalid", "A denúncia ou evidência é inválida.");
    private static readonly DomainError Conflict = new("reports.conflict", "Já existe uma denúncia ativa para este alvo.");
    private static readonly DomainError NotFound = new("reports.not_found", "Denúncia ou evidência não encontrada.");

    public async Task<Result<ReportResponse>> CreateAsync(Guid reporterUserId, CreateReportRequest request, CancellationToken cancellationToken)
    {
        ReportTargetType targetType = Enum.Parse<ReportTargetType>(request.TargetType.ToString());
        Guid? ownerUserId = await TargetOwnerAsync(targetType, request.TargetId, cancellationToken);
        if (ownerUserId is null || ownerUserId == reporterUserId) return Result.Failure<ReportResponse>(Invalid);
        bool duplicate = await dbContext.Reports.AnyAsync(x => x.ReporterUserId == reporterUserId && x.TargetType == targetType && x.TargetId == request.TargetId && (x.Status == ReportStatus.Submitted || x.Status == ReportStatus.UnderReview), cancellationToken);
        if (duplicate) return Result.Failure<ReportResponse>(Conflict);
        Report report;
        try { report = new Report(reporterUserId, targetType, request.TargetId, Enum.Parse<ReportReason>(request.Reason.ToString()), request.Description, timeProvider.GetUtcNow()); }
        catch (ArgumentException) { return Result.Failure<ReportResponse>(Invalid); }
        dbContext.Reports.Add(report);
        try { await dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException) { return Result.Failure<ReportResponse>(Conflict); }
        return Result.Success(ToResponse(report, 0));
    }

    public async Task<IReadOnlyCollection<ReportResponse>> ListMineAsync(Guid reporterUserId, CancellationToken cancellationToken) =>
        await Query().Where(x => dbContext.Reports.Any(r => r.Id == x.Id && r.ReporterUserId == reporterUserId)).OrderByDescending(x => x.CreatedAtUtc).Take(100).ToArrayAsync(cancellationToken);

    public async Task<Result<ReportEvidenceResponse>> AddEvidenceAsync(Guid reporterUserId, Guid reportId, Stream content, long length, CancellationToken cancellationToken)
    {
        Report? report = await dbContext.Reports.SingleOrDefaultAsync(x => x.Id == reportId && x.ReporterUserId == reporterUserId && x.Status != ReportStatus.Dismissed, cancellationToken);
        if (report is null) return Result.Failure<ReportEvidenceResponse>(NotFound);
        if (length is <= 0 or > MaximumEvidenceLength || await dbContext.ReportEvidence.CountAsync(x => x.ReportId == reportId, cancellationToken) >= 5) return Result.Failure<ReportEvidenceResponse>(Invalid);
        await using MemoryStream buffer = new();
        await content.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length != length || buffer.Length > MaximumEvidenceLength) return Result.Failure<ReportEvidenceResponse>(Invalid);
        (string? contentType, string? extension) = Detect(buffer.GetBuffer().AsSpan(0, (int)Math.Min(buffer.Length, 12)));
        if (contentType is null) return Result.Failure<ReportEvidenceResponse>(Invalid);
        string relative = Path.Combine("reports", reportId.ToString("N"), $"{Guid.NewGuid():N}{extension}");
        string fullPath = Path.Combine(environment.ContentRootPath, ".dev-storage", relative);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllBytesAsync(fullPath, buffer.ToArray(), cancellationToken);
        ReportEvidence evidence = new(reportId, relative, contentType, length, timeProvider.GetUtcNow());
        dbContext.ReportEvidence.Add(evidence);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(new ReportEvidenceResponse(evidence.Id, evidence.ContentType, evidence.Length, evidence.CreatedAtUtc));
    }

    public async Task<IReadOnlyCollection<ReportResponse>> QueueAsync(CancellationToken cancellationToken) =>
        await Query().Where(x => x.Status == ReportStatus.Submitted.ToString() || x.Status == ReportStatus.UnderReview.ToString()).OrderBy(x => x.CreatedAtUtc).Take(200).ToArrayAsync(cancellationToken);

    public async Task<Result<ReportResponse>> TransitionAsync(Guid moderatorUserId, Guid reportId, string transition, CancellationToken cancellationToken)
    {
        Report? report = await dbContext.Reports.SingleOrDefaultAsync(x => x.Id == reportId, cancellationToken);
        if (report is null) return Result.Failure<ReportResponse>(NotFound);
        try
        {
            if (transition == "review") report.StartReview(moderatorUserId, timeProvider.GetUtcNow());
            else if (transition == "dismiss") report.Dismiss(moderatorUserId, timeProvider.GetUtcNow());
            else return Result.Failure<ReportResponse>(Invalid);
        }
        catch (InvalidOperationException) { return Result.Failure<ReportResponse>(Conflict); }
        await dbContext.SaveChangesAsync(cancellationToken);
        int count = await dbContext.ReportEvidence.CountAsync(x => x.ReportId == report.Id, cancellationToken);
        return Result.Success(ToResponse(report, count));
    }

    public async Task<Result<ProtectedEvidenceFile>> GetEvidenceAsync(Guid evidenceId, CancellationToken cancellationToken)
    {
        ReportEvidence? evidence = await dbContext.ReportEvidence.AsNoTracking().SingleOrDefaultAsync(x => x.Id == evidenceId, cancellationToken);
        if (evidence is null) return Result.Failure<ProtectedEvidenceFile>(NotFound);
        string fullPath = Path.Combine(environment.ContentRootPath, ".dev-storage", evidence.StorageKey);
        if (!File.Exists(fullPath)) return Result.Failure<ProtectedEvidenceFile>(NotFound);
        return Result.Success(new ProtectedEvidenceFile(File.OpenRead(fullPath), evidence.ContentType));
    }

    private IQueryable<ReportResponse> Query() =>
        dbContext.Reports.AsNoTracking().Select(x => new ReportResponse(x.Id, x.TargetType.ToString(), x.TargetId, x.Reason.ToString(), x.Description, x.Status.ToString(), x.CreatedAtUtc, dbContext.ReportEvidence.Count(e => e.ReportId == x.Id)));

    private async Task<Guid?> TargetOwnerAsync(ReportTargetType type, Guid targetId, CancellationToken cancellationToken) => type switch
    {
        ReportTargetType.User => await dbContext.Users.Where(x => x.Id == targetId).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(cancellationToken),
        ReportTargetType.Dog => await dbContext.Dogs.Where(x => x.Id == targetId).Select(x => (Guid?)x.OwnerUserId).SingleOrDefaultAsync(cancellationToken),
        ReportTargetType.AdoptionProfile => await dbContext.AdoptionProfiles.Where(x => x.Id == targetId).Select(x => (Guid?)x.PublisherUserId).SingleOrDefaultAsync(cancellationToken),
        ReportTargetType.ChatMessage => await dbContext.ChatMessages.Where(x => x.Id == targetId).Select(x => (Guid?)x.SenderUserId).SingleOrDefaultAsync(cancellationToken),
        _ => null,
    };

    private static (string?, string?) Detect(ReadOnlySpan<byte> header)
    {
        if (header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) return ("image/jpeg", ".jpg");
        if (header.Length >= 8 && header[..8].SequenceEqual(PngSignature)) return ("image/png", ".png");
        if (header.Length >= 5 && header[..5].SequenceEqual("%PDF-"u8)) return ("application/pdf", ".pdf");
        return (null, null);
    }

    private static ReportResponse ToResponse(Report report, int evidenceCount) =>
        new(report.Id, report.TargetType.ToString(), report.TargetId, report.Reason.ToString(), report.Description, report.Status.ToString(), report.CreatedAtUtc, evidenceCount);

    private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];
}
