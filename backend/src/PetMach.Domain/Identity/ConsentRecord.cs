namespace PetMach.Domain.Identity;

public sealed class ConsentRecord
{
    private ConsentRecord() { }

    public ConsentRecord(Guid userId, string termsVersion, string privacyVersion, DateTimeOffset acceptedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(termsVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(privacyVersion);
        if (userId == Guid.Empty) throw new ArgumentException("O usuário é obrigatório.", nameof(userId));

        Id = Guid.NewGuid();
        UserId = userId;
        TermsVersion = termsVersion;
        PrivacyVersion = privacyVersion;
        AcceptedAtUtc = acceptedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TermsVersion { get; private set; } = string.Empty;
    public string PrivacyVersion { get; private set; } = string.Empty;
    public DateTimeOffset AcceptedAtUtc { get; private set; }
}
