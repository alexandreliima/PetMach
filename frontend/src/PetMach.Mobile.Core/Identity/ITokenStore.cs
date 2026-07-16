namespace PetMach.Mobile.Core.Identity;

public interface ITokenStore
{
    Task<StoredSession?> GetAsync(CancellationToken cancellationToken);
    Task SaveAsync(StoredSession session, CancellationToken cancellationToken);
    Task ClearAsync(CancellationToken cancellationToken);
}
