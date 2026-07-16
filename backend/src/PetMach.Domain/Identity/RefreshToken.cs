using PetMach.Domain.SharedKernel;

namespace PetMach.Domain.Identity;

public sealed class RefreshToken
{
    private RefreshToken() { }

    private RefreshToken(Guid id, Guid userId, Guid familyId, string tokenHash, DateTimeOffset createdAtUtc, DateTimeOffset expiresAtUtc)
    {
        Id = id;
        UserId = userId;
        FamilyId = familyId;
        TokenHash = tokenHash;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid FamilyId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset? UsedAtUtc { get; private set; }
    public DateTimeOffset? RevokedAtUtc { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }

    public bool IsActive(DateTimeOffset now) => UsedAtUtc is null && RevokedAtUtc is null && ExpiresAtUtc > now;

    public static Result<RefreshToken> Issue(Guid userId, Guid familyId, string tokenHash, DateTimeOffset now, DateTimeOffset expiresAt)
    {
        if (userId == Guid.Empty || familyId == Guid.Empty || string.IsNullOrWhiteSpace(tokenHash) || expiresAt <= now)
        {
            return Result.Failure<RefreshToken>(IdentityErrors.InvalidToken);
        }

        return Result.Success(new RefreshToken(Guid.NewGuid(), userId, familyId, tokenHash, now, expiresAt));
    }

    public Result Consume(Guid replacementId, DateTimeOffset now)
    {
        if (!IsActive(now) || replacementId == Guid.Empty)
        {
            return Result.Failure(IdentityErrors.InvalidToken);
        }

        UsedAtUtc = now;
        ReplacedByTokenId = replacementId;
        return Result.Success();
    }

    public void Revoke(DateTimeOffset now)
    {
        if (RevokedAtUtc is null)
        {
            RevokedAtUtc = now;
        }
    }
}
