namespace PetMach.Domain.Partners;

public sealed class SpaceAvailability
{
    private SpaceAvailability() { }

    public SpaceAvailability(Guid spaceId, DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc, DateTimeOffset now)
    {
        if (spaceId == Guid.Empty || startsAtUtc <= now || endsAtUtc <= startsAtUtc || endsAtUtc - startsAtUtc > TimeSpan.FromDays(7))
            throw new ArgumentException("Janela de disponibilidade inválida.");
        Id = Guid.NewGuid();
        SpaceId = spaceId;
        StartsAtUtc = startsAtUtc.ToUniversalTime();
        EndsAtUtc = endsAtUtc.ToUniversalTime();
        IsAvailable = true;
        CreatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public Guid SpaceId { get; private set; }
    public DateTimeOffset StartsAtUtc { get; private set; }
    public DateTimeOffset EndsAtUtc { get; private set; }
    public bool IsAvailable { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public void Close() => IsAvailable = false;
}
