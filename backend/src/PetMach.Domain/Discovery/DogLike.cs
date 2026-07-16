namespace PetMach.Domain.Discovery;

public sealed class DogLike
{
    private DogLike() { }

    public DogLike(Guid sourceDogId, Guid targetDogId, DateTimeOffset now)
    {
        if (sourceDogId == Guid.Empty || targetDogId == Guid.Empty || sourceDogId == targetDogId)
            throw new ArgumentException("Like inválido.");

        Id = Guid.NewGuid();
        SourceDogId = sourceDogId;
        TargetDogId = targetDogId;
        CreatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public Guid SourceDogId { get; private set; }
    public Guid TargetDogId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
