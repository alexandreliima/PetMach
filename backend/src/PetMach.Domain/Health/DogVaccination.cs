namespace PetMach.Domain.Health;

public sealed class DogVaccination
{
    private DogVaccination() { }
    public DogVaccination(Guid dogId, string vaccineName, DateOnly appliedOn, DateOnly? nextDoseOn, DateTimeOffset now)
    {
        if (dogId == Guid.Empty || string.IsNullOrWhiteSpace(vaccineName) || vaccineName.Length > 150 || nextDoseOn < appliedOn) throw new ArgumentException("Vacinação inválida.");
        Id = Guid.NewGuid(); DogId = dogId; VaccineName = vaccineName.Trim(); AppliedOn = appliedOn; NextDoseOn = nextDoseOn; CreatedAtUtc = now;
    }
    public Guid Id { get; private set; }
    public Guid DogId { get; private set; }
    public string VaccineName { get; private set; } = string.Empty;
    public DateOnly AppliedOn { get; private set; }
    public DateOnly? NextDoseOn { get; private set; }
    public string? ProtectedProofKey { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public void AttachProof(string protectedProofKey)
    {
        if (string.IsNullOrWhiteSpace(protectedProofKey) || protectedProofKey.Length > 300)
            throw new ArgumentException("Comprovante inválido.", nameof(protectedProofKey));

        ProtectedProofKey = protectedProofKey;
    }
}
