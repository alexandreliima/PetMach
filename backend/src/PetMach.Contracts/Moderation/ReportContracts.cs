namespace PetMach.Contracts.Moderation;

public enum ReportTargetTypeContract { User, Dog, AdoptionProfile, ChatMessage }
public enum ReportReasonContract { Harassment, Fraud, UnsafeContent, AnimalWelfare, Spam, Other }
public sealed record CreateReportRequest(ReportTargetTypeContract TargetType, Guid TargetId, ReportReasonContract Reason, string Description);
public sealed record ReportResponse(Guid Id, string TargetType, Guid TargetId, string Reason, string Description, string Status, DateTimeOffset CreatedAtUtc, int EvidenceCount);
public sealed record ReportEvidenceResponse(Guid Id, string ContentType, long Length, DateTimeOffset CreatedAtUtc);
