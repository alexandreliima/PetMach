namespace PetMach.Domain.Adoption;

public enum AdoptionStatus { Available, InProgress, Adopted, Suspended }

public sealed class AdoptionProfile
{
    private AdoptionProfile() { }

    public AdoptionProfile(Guid dogId, Guid publisherUserId, string story, string requirements, string termsVersion, DateTimeOffset now)
    {
        if (dogId == Guid.Empty || publisherUserId == Guid.Empty || string.IsNullOrWhiteSpace(story) || story.Trim().Length > 2000 ||
            string.IsNullOrWhiteSpace(requirements) || requirements.Trim().Length > 1500 || string.IsNullOrWhiteSpace(termsVersion))
            throw new ArgumentException("Publicação de adoção inválida.");
        Id = Guid.NewGuid(); DogId = dogId; PublisherUserId = publisherUserId; Story = story.Trim(); Requirements = requirements.Trim();
        TermsVersion = termsVersion.Trim(); TermsAcceptedAtUtc = now.ToUniversalTime(); Status = AdoptionStatus.Available;
        CreatedAtUtc = TermsAcceptedAtUtc; UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid DogId { get; private set; }
    public Guid PublisherUserId { get; private set; }
    public string Story { get; private set; } = string.Empty;
    public string Requirements { get; private set; } = string.Empty;
    public string TermsVersion { get; private set; } = string.Empty;
    public DateTimeOffset TermsAcceptedAtUtc { get; private set; }
    public AdoptionStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public void Suspend(DateTimeOffset now)
    {
        if (Status is AdoptionStatus.Adopted or AdoptionStatus.Suspended) throw new InvalidOperationException("A publicação não pode ser suspensa.");
        Status = AdoptionStatus.Suspended; UpdatedAtUtc = now.ToUniversalTime();
    }

    public void MarkInProgress(DateTimeOffset now)
    {
        if (Status != AdoptionStatus.Available) throw new InvalidOperationException("A publicação não está disponível.");
        Status = AdoptionStatus.InProgress; UpdatedAtUtc = now.ToUniversalTime();
    }
}
