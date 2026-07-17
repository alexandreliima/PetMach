using Microsoft.AspNetCore.SignalR.Client;
using PetMach.Mobile.Core.Features;
using PetMach.Mobile.Core.Identity;

namespace PetMach.Mobile.Features;

public sealed class SignalRChatRealtimeClient(
    HttpClient httpClient,
    AuthenticationSession session,
    AuthenticatedConnectionManager connectionManager) : IChatRealtimeClient
{
    private static readonly TimeSpan SessionOperationTimeout = TimeSpan.FromSeconds(30);
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
                options.AccessTokenProvider = async () =>
                {
                    using CancellationTokenSource timeout = new(SessionOperationTimeout);
                    return await session.GetAccessTokenAsync(timeout.Token);
                };
            })
            .WithAutomaticReconnect([TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)])
            .Build();
        connection.On<ChatMessageModel>("MessageReceived", message => MessageReceived?.Invoke(message) ?? Task.CompletedTask);
        connection.Reconnecting += _ => Reconnecting?.Invoke() ?? Task.CompletedTask;
        connection.Reconnected += async _ =>
        {
            using CancellationTokenSource timeout = new(SessionOperationTimeout);
            await JoinAsync(timeout.Token);
            if (Reconnected is not null) await Reconnected.Invoke();
        };
        try
        {
            await connection.StartAsync(cancellationToken);
            await JoinAsync(cancellationToken);
            connectionManager.Register(this);
        }
        catch
        {
            using CancellationTokenSource timeout = new(SessionOperationTimeout);
            await StopAsync(timeout.Token);
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (connection is null)
        {
            connectionManager.Unregister(this);
            return;
        }

        try
        {
            await connection.StopAsync(cancellationToken);
            await connection.DisposeAsync();
        }
        finally
        {
            connection = null;
            activeConversationId = Guid.Empty;
            connectionManager.Unregister(this);
        }
    }

    public async ValueTask DisposeAsync()
    {
        using CancellationTokenSource timeout = new(SessionOperationTimeout);
        await StopAsync(timeout.Token);
    }

    private Task JoinAsync(CancellationToken cancellationToken) =>
        connection is null || activeConversationId == Guid.Empty
            ? Task.CompletedTask
            : connection.InvokeAsync("JoinConversation", activeConversationId, cancellationToken);
}
