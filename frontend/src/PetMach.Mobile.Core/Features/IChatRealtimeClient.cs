namespace PetMach.Mobile.Core.Features;

public interface IChatRealtimeClient : IAsyncDisposable
{
    event Func<ChatMessageModel, Task>? MessageReceived;
    event Func<Task>? Reconnecting;
    event Func<Task>? Reconnected;
    Task StartAsync(Guid conversationId, CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
