namespace PetMach.Domain.Moderation;

public enum ReportTargetType { User, Dog, AdoptionProfile, ChatMessage }
public enum ReportReason { Harassment, Fraud, UnsafeContent, AnimalWelfare, Spam, Other }
public enum ReportStatus { Submitted, UnderReview, Actioned, Dismissed }

public sealed class Report
{
    private Report() { }
    public Report(Guid reporterUserId, ReportTargetType targetType, Guid targetId, ReportReason reason, string description, DateTimeOffset now)
    {
        if (reporterUserId == Guid.Empty || targetId == Guid.Empty || string.IsNullOrWhiteSpace(description) || description.Trim().Length > 2000)
            throw new ArgumentException("Denúncia inválida.");
        Id = Guid.NewGuid(); ReporterUserId = reporterUserId; TargetType = targetType; TargetId = targetId; Reason = reason;
        Description = description.Trim(); Status = ReportStatus.Submitted; CreatedAtUtc = now.ToUniversalTime(); UpdatedAtUtc = CreatedAtUtc;
    }
    public Guid Id { get; private set; }
    public Guid ReporterUserId { get; private set; }
    public ReportTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public ReportReason Reason { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public ReportStatus Status { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public void StartReview(Guid moderatorUserId, DateTimeOffset now)
    {
        if (moderatorUserId == Guid.Empty || Status != ReportStatus.Submitted) throw new InvalidOperationException("Denúncia não pode entrar em revisão.");
        Status = ReportStatus.UnderReview; ReviewedByUserId = moderatorUserId; UpdatedAtUtc = now.ToUniversalTime();
    }
    public void Dismiss(Guid moderatorUserId, DateTimeOffset now)
    {
        if (moderatorUserId == Guid.Empty || Status is not (ReportStatus.Submitted or ReportStatus.UnderReview)) throw new InvalidOperationException("Denúncia não pode ser arquivada.");
        Status = ReportStatus.Dismissed; ReviewedByUserId = moderatorUserId; UpdatedAtUtc = now.ToUniversalTime();
    }
    public void MarkActioned(Guid moderatorUserId, DateTimeOffset now)
    {
        if (moderatorUserId == Guid.Empty || Status != ReportStatus.UnderReview) throw new InvalidOperationException("Denúncia não pode receber ação.");
        Status = ReportStatus.Actioned; ReviewedByUserId = moderatorUserId; UpdatedAtUtc = now.ToUniversalTime();
    }
}
