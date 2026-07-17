using System.Text.Json;
using PetMach.Mobile.Core.Identity;

namespace PetMach.Mobile.Identity;

public sealed class SecureTokenStore : ITokenStore, IDisposable
{
    private const string SessionKey = "petmach.auth.session";
    private readonly SemaphoreSlim gate = new(1, 1);

    public async Task<StoredSession?> GetAsync(CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            string? json = await SecureStorage.Default.GetAsync(SessionKey);
            cancellationToken.ThrowIfCancellationRequested();
            return json is null ? null : JsonSerializer.Deserialize<StoredSession>(json);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task SaveAsync(StoredSession session, CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await SecureStorage.Default.SetAsync(SessionKey, JsonSerializer.Serialize(session));
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            SecureStorage.Default.Remove(SessionKey);
        }
        finally
        {
            gate.Release();
        }
    }

    public void Dispose() => gate.Dispose();
}
