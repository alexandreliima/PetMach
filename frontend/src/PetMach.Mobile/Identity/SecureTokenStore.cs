using System.Text.Json;
using PetMach.Mobile.Core.Identity;

namespace PetMach.Mobile.Identity;

public sealed class SecureTokenStore : ITokenStore
{
    private const string SessionKey = "petmach.auth.session";

    public async Task<StoredSession?> GetAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string? json = await SecureStorage.Default.GetAsync(SessionKey);
        return json is null ? null : JsonSerializer.Deserialize<StoredSession>(json);
    }

    public Task SaveAsync(StoredSession session, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return SecureStorage.Default.SetAsync(SessionKey, JsonSerializer.Serialize(session));
    }

    public Task ClearAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SecureStorage.Default.Remove(SessionKey);
        return Task.CompletedTask;
    }
}
