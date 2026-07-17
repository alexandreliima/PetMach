namespace PetMach.Domain.Moderation;

public enum ModerationActionType { SuspendUser, SuspendDog, SuspendAdoptionProfile }

public sealed class ModerationAction
{
    private ModerationAction() { }
    public ModerationAction(Guid reportId, Guid moderatorUserId, ModerationActionType actionType, ReportTargetType targetType, Guid targetId, DateTimeOffset now)
    {
        if (reportId == Guid.Empty || moderatorUserId == Guid.Empty || targetId == Guid.Empty) throw new ArgumentException("Ação de moderação inválida.");
        Id = Guid.NewGuid(); ReportId = reportId; ModeratorUserId = moderatorUserId; ActionType = actionType; TargetType = targetType; TargetId = targetId; OccurredAtUtc = now.ToUniversalTime();
    }
    public Guid Id { get; private set; }
    public Guid ReportId { get; private set; }
    public Guid ModeratorUserId { get; private set; }
    public ModerationActionType ActionType { get; private set; }
    public ReportTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
}
