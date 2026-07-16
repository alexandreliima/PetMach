namespace PetMach.Domain.Partners;

public sealed class PartnerSpace
{
    private PartnerSpace() { }

    public PartnerSpace(Guid establishmentId, string name, string description, int capacity, decimal informationalPrice, DateTimeOffset now)
    {
        if (establishmentId == Guid.Empty || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description) || name.Trim().Length > 120 || description.Trim().Length > 1000 || capacity is < 1 or > 1000 || informationalPrice < 0)
            throw new ArgumentException("Espaço inválido.");
        Id = Guid.NewGuid();
        EstablishmentId = establishmentId;
        Name = name.Trim();
        Description = description.Trim();
        Capacity = capacity;
        InformationalPrice = informationalPrice;
        IsActive = true;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public Guid EstablishmentId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int Capacity { get; private set; }
    public decimal InformationalPrice { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
}
