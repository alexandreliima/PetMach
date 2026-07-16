namespace PetMach.Domain.Discovery;

public sealed class DogPass
{
    private DogPass() { }

    public DogPass(Guid sourceDogId, Guid targetDogId, DateTimeOffset now)
    {
        if (sourceDogId == Guid.Empty || targetDogId == Guid.Empty || sourceDogId == targetDogId)
            throw new ArgumentException("Perfil ignorado inválido.");

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
