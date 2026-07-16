namespace PetMach.Domain.Health;

public sealed class DewormingRecord
{
    private DewormingRecord() { }
    public DewormingRecord(Guid dogId, string productName, DateOnly appliedOn, DateOnly? nextDoseOn, DateTimeOffset now)
    {
        if (dogId == Guid.Empty || string.IsNullOrWhiteSpace(productName) || productName.Length > 150 || nextDoseOn < appliedOn) throw new ArgumentException("Vermifugação inválida.");
        Id = Guid.NewGuid(); DogId = dogId; ProductName = productName.Trim(); AppliedOn = appliedOn; NextDoseOn = nextDoseOn; CreatedAtUtc = now;
    }
    public Guid Id { get; private set; }
    public Guid DogId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public DateOnly AppliedOn { get; private set; }
    public DateOnly? NextDoseOn { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
