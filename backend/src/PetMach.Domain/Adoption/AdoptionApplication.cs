namespace PetMach.Domain.Adoption;

public enum AdoptionApplicationStatus { Submitted, UnderReview, Approved, Rejected, Withdrawn }

public sealed class AdoptionApplication
{
    private AdoptionApplication() { }
    public AdoptionApplication(Guid profileId, Guid applicantUserId, string motivation, string experience, string housingContext, string termsVersion, DateTimeOffset now)
    {
        if (profileId == Guid.Empty || applicantUserId == Guid.Empty || string.IsNullOrWhiteSpace(motivation) || motivation.Trim().Length > 2000 ||
            string.IsNullOrWhiteSpace(experience) || experience.Trim().Length > 1500 || string.IsNullOrWhiteSpace(housingContext) || housingContext.Trim().Length > 1000 || string.IsNullOrWhiteSpace(termsVersion))
            throw new ArgumentException("Candidatura de adoção inválida.");
        Id = Guid.NewGuid(); ProfileId = profileId; ApplicantUserId = applicantUserId; Motivation = motivation.Trim(); Experience = experience.Trim();
        HousingContext = housingContext.Trim(); TermsVersion = termsVersion.Trim(); TermsAcceptedAtUtc = now.ToUniversalTime(); Status = AdoptionApplicationStatus.Submitted;
        CreatedAtUtc = TermsAcceptedAtUtc; UpdatedAtUtc = CreatedAtUtc;
    }
    public Guid Id { get; private set; }
    public Guid ProfileId { get; private set; }
    public Guid ApplicantUserId { get; private set; }
    public string Motivation { get; private set; } = string.Empty;
    public string Experience { get; private set; } = string.Empty;
    public string HousingContext { get; private set; } = string.Empty;
    public string TermsVersion { get; private set; } = string.Empty;
    public DateTimeOffset TermsAcceptedAtUtc { get; private set; }
    public AdoptionApplicationStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public void StartReview(DateTimeOffset now) => Transition(AdoptionApplicationStatus.Submitted, AdoptionApplicationStatus.UnderReview, now);
    public void Approve(DateTimeOffset now) => Transition(AdoptionApplicationStatus.UnderReview, AdoptionApplicationStatus.Approved, now);
    public void Reject(DateTimeOffset now)
    {
        if (Status is not (AdoptionApplicationStatus.Submitted or AdoptionApplicationStatus.UnderReview)) throw new InvalidOperationException("A candidatura não pode ser rejeitada.");
        Status = AdoptionApplicationStatus.Rejected; UpdatedAtUtc = now.ToUniversalTime();
    }
    public void Withdraw(DateTimeOffset now)
    {
        if (Status is not (AdoptionApplicationStatus.Submitted or AdoptionApplicationStatus.UnderReview)) throw new InvalidOperationException("A candidatura não pode ser retirada.");
        Status = AdoptionApplicationStatus.Withdrawn; UpdatedAtUtc = now.ToUniversalTime();
    }
    private void Transition(AdoptionApplicationStatus expected, AdoptionApplicationStatus next, DateTimeOffset now)
    {
        if (Status != expected) throw new InvalidOperationException("Transição de candidatura inválida.");
        Status = next; UpdatedAtUtc = now.ToUniversalTime();
    }
}
