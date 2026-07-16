namespace PetMach.Domain.Moderation;

public sealed class BlockedUser
{
    private BlockedUser() { }

    public BlockedUser(Guid userId, Guid blockedUserId, DateTimeOffset now)
    {
        if (userId == Guid.Empty || blockedUserId == Guid.Empty || userId == blockedUserId)
            throw new ArgumentException("Bloqueio inválido.");

        Id = Guid.NewGuid();
        UserId = userId;
        BlockedUserId = blockedUserId;
        CreatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid BlockedUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
