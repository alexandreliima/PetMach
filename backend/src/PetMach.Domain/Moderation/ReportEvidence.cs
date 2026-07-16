namespace PetMach.Domain.Moderation;

public sealed class ReportEvidence
{
    private ReportEvidence() { }
    public ReportEvidence(Guid reportId, string storageKey, string contentType, long length, DateTimeOffset now)
    {
        if (reportId == Guid.Empty || string.IsNullOrWhiteSpace(storageKey) || string.IsNullOrWhiteSpace(contentType) || length <= 0) throw new ArgumentException("Evidência inválida.");
        Id = Guid.NewGuid(); ReportId = reportId; StorageKey = storageKey; ContentType = contentType; Length = length; CreatedAtUtc = now.ToUniversalTime();
    }
    public Guid Id { get; private set; }
    public Guid ReportId { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long Length { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
