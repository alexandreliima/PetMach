namespace PetMach.Domain.Partners;

public sealed class PartnerEstablishment
{
    private PartnerEstablishment() { }

    public PartnerEstablishment(Guid ownerUserId, string legalName, string displayName, string registrationNumber, string city, string state, string timeZoneId, DateTimeOffset now)
    {
        if (ownerUserId == Guid.Empty || string.IsNullOrWhiteSpace(legalName) || string.IsNullOrWhiteSpace(displayName) ||
            string.IsNullOrWhiteSpace(registrationNumber) || string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(timeZoneId) ||
            legalName.Trim().Length > 180 || displayName.Trim().Length > 120 || registrationNumber.Trim().Length > 32 || city.Trim().Length > 120 || state.Trim().Length > 50 || timeZoneId.Trim().Length > 100)
            throw new ArgumentException("Parceiro inválido.");
        Id = Guid.NewGuid();
        OwnerUserId = ownerUserId;
        LegalName = legalName.Trim();
        DisplayName = displayName.Trim();
        RegistrationNumber = registrationNumber.Trim();
        City = city.Trim();
        State = state.Trim().ToUpperInvariant();
        TimeZoneId = timeZoneId.Trim();
        IsActive = true;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public Guid OwnerUserId { get; private set; }
    public string LegalName { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string RegistrationNumber { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string TimeZoneId { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
}
