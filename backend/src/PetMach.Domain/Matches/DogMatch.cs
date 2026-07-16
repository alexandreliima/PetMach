namespace PetMach.Domain.Matches;

public sealed class DogMatch
{
    private DogMatch() { }

    public DogMatch(Guid firstDogId, Guid secondDogId, DateTimeOffset now)
    {
        if (firstDogId == Guid.Empty || secondDogId == Guid.Empty || firstDogId == secondDogId)
            throw new ArgumentException("Match inválido.");

        (DogAId, DogBId) = firstDogId.CompareTo(secondDogId) < 0 ? (firstDogId, secondDogId) : (secondDogId, firstDogId);
        Id = Guid.NewGuid();
        CreatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public Guid DogAId { get; private set; }
    public Guid DogBId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? EndedAtUtc { get; private set; }
    public bool IsActive => EndedAtUtc is null;

    public void End(DateTimeOffset now) => EndedAtUtc ??= now;
}
