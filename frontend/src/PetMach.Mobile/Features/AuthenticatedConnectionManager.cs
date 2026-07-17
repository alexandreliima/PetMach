using System.Collections.Concurrent;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Features;

public sealed class AuthenticatedConnectionManager : IAuthenticatedConnectionManager
{
    private readonly ConcurrentDictionary<SignalRChatRealtimeClient, byte> connections = [];

    internal void Register(SignalRChatRealtimeClient connection) =>
        connections.TryAdd(connection, 0);

    internal void Unregister(SignalRChatRealtimeClient connection) =>
        connections.TryRemove(connection, out _);

    public Task StopAuthenticatedConnectionsAsync(CancellationToken cancellationToken) =>
        Task.WhenAll(connections.Keys.Select(connection => connection.StopAsync(cancellationToken)));
}
