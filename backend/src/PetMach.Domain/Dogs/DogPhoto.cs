namespace PetMach.Domain.Dogs;

public sealed class DogPhoto
{
    private DogPhoto() { }
    public DogPhoto(Guid dogId, string storageKey, string contentType, long length, bool isPrimary, DateTimeOffset now)
    { Id = Guid.NewGuid(); DogId = dogId; StorageKey = storageKey; ContentType = contentType; Length = length; IsPrimary = isPrimary; CreatedAtUtc = now; }
    public Guid Id { get; private set; }
    public Guid DogId { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long Length { get; private set; }
    public bool IsPrimary { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public void SetPrimary(bool value) => IsPrimary = value;
}
