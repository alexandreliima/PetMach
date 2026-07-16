namespace PetMach.Domain.Adoption;

public sealed class AdoptionApplicationHistory
{
    private AdoptionApplicationHistory() { }
    public AdoptionApplicationHistory(Guid applicationId, Guid actorUserId, AdoptionApplicationStatus? fromStatus, AdoptionApplicationStatus toStatus, DateTimeOffset now)
    {
        if (applicationId == Guid.Empty || actorUserId == Guid.Empty) throw new ArgumentException("Histórico de candidatura inválido.");
        Id = Guid.NewGuid(); ApplicationId = applicationId; ActorUserId = actorUserId; FromStatus = fromStatus; ToStatus = toStatus; OccurredAtUtc = now.ToUniversalTime();
    }
    public Guid Id { get; private set; }
    public Guid ApplicationId { get; private set; }
    public Guid ActorUserId { get; private set; }
    public AdoptionApplicationStatus? FromStatus { get; private set; }
    public AdoptionApplicationStatus ToStatus { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
}
