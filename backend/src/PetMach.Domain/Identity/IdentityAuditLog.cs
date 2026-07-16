namespace PetMach.Domain.Identity;

public sealed class IdentityAuditLog
{
    private IdentityAuditLog() { }

    public IdentityAuditLog(Guid? actorUserId, string action, Guid targetUserId, DateTimeOffset occurredAtUtc, string result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(result);
        if (targetUserId == Guid.Empty) throw new ArgumentException("O alvo é obrigatório.", nameof(targetUserId));
        Id = Guid.NewGuid();
        ActorUserId = actorUserId;
        Action = action;
        TargetUserId = targetUserId;
        OccurredAtUtc = occurredAtUtc;
        Result = result;
    }

    public Guid Id { get; private set; }
    public Guid? ActorUserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public Guid TargetUserId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public string Result { get; private set; } = string.Empty;
}
