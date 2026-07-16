using Microsoft.AspNetCore.SignalR.Client;
using PetMach.Mobile.Core.Features;
using PetMach.Mobile.Core.Identity;

namespace PetMach.Mobile.Features;

public sealed class SignalRChatRealtimeClient(HttpClient httpClient, AuthenticationSession session) : IChatRealtimeClient
{
    private HubConnection? connection;
    private Guid activeConversationId;

    public event Func<ChatMessageModel, Task>? MessageReceived;
    public event Func<Task>? Reconnecting;
    public event Func<Task>? Reconnected;

    public async Task StartAsync(Guid conversationId, CancellationToken cancellationToken)
    {
        await StopAsync(cancellationToken);
        activeConversationId = conversationId;
        connection = new HubConnectionBuilder()
            .WithUrl(new Uri(httpClient.BaseAddress!, "hubs/chat"), options =>
            {
                options.AccessTokenProvider = () => session.GetAccessTokenAsync(CancellationToken.None);
            })
            .WithAutomaticReconnect([TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)])
            .Build();
        connection.On<ChatMessageModel>("MessageReceived", message => MessageReceived?.Invoke(message) ?? Task.CompletedTask);
        connection.Reconnecting += _ => Reconnecting?.Invoke() ?? Task.CompletedTask;
        connection.Reconnected += async _ =>
        {
            await JoinAsync(CancellationToken.None);
            if (Reconnected is not null) await Reconnected.Invoke();
        };
        await connection.StartAsync(cancellationToken);
        await JoinAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (connection is null) return;
        await connection.StopAsync(cancellationToken);
        await connection.DisposeAsync();
        connection = null;
        activeConversationId = Guid.Empty;
    }

    public async ValueTask DisposeAsync() => await StopAsync(CancellationToken.None);

    private Task JoinAsync(CancellationToken cancellationToken) =>
        connection is null || activeConversationId == Guid.Empty
            ? Task.CompletedTask
            : connection.InvokeAsync("JoinConversation", activeConversationId, cancellationToken);
}
