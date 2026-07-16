namespace PetMach.Domain.Discovery;

public sealed class DogPreference
{
    private DogPreference() { }

    public DogPreference(Guid dogId, int? maximumDistanceKm, int? minimumAgeYears, int? maximumAgeYears, DateTimeOffset now)
    {
        if (dogId == Guid.Empty || maximumDistanceKm is <= 0 or > 500 || minimumAgeYears is < 0 or > 30 || maximumAgeYears is < 0 or > 30 || minimumAgeYears > maximumAgeYears)
            throw new ArgumentException("Preferências inválidas.");

        Id = Guid.NewGuid();
        DogId = dogId;
        MaximumDistanceKm = maximumDistanceKm;
        MinimumAgeYears = minimumAgeYears;
        MaximumAgeYears = maximumAgeYears;
        UpdatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public Guid DogId { get; private set; }
    public int? MaximumDistanceKm { get; private set; }
    public int? MinimumAgeYears { get; private set; }
    public int? MaximumAgeYears { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
}
