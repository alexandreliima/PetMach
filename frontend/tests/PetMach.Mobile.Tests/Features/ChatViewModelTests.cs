using FluentAssertions;
using NSubstitute;
using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile.Tests.Features;

public sealed class ChatViewModelTests
{
    [Fact]
    public async Task RealtimeMessageShouldBeDeduplicatedAgainstHistoryAndSendResponse()
    {
        Guid conversationId = Guid.NewGuid();
        ChatMessageModel existing = new(Guid.NewGuid(), conversationId, Guid.NewGuid(), "Olá", DateTimeOffset.UtcNow);
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        api.GetMessagesAsync(conversationId, 1, Arg.Any<CancellationToken>())
            .Returns(new ChatMessagePageModel([existing], 1, 30, false));
        api.SendMessageAsync(conversationId, "Nova", Arg.Any<CancellationToken>())
            .Returns(new ChatMessageModel(Guid.NewGuid(), conversationId, Guid.NewGuid(), "Nova", DateTimeOffset.UtcNow));
        RealtimeClient realtime = new();
        ChatViewModel viewModel = new(api, realtime);

        await viewModel.InitializeAsync(conversationId);
        await realtime.PublishAsync(existing);
        viewModel.Draft = "Nova";
        await viewModel.SendCommand.ExecuteAsync(null);
        ChatMessageModel sent = viewModel.Messages.Last();
        await realtime.PublishAsync(sent);

        viewModel.Messages.Should().HaveCount(2);
        viewModel.Messages.Select(x => x.Id).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task ReconnectionShouldExposeConnectionStatus()
    {
        SynchronizationContext? originalContext = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(new ImmediateSynchronizationContext());
        try
        {
            Guid conversationId = Guid.NewGuid();
            IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
            api.GetMessagesAsync(conversationId, 1, Arg.Any<CancellationToken>()).Returns(new ChatMessagePageModel([], 1, 30, false));
            RealtimeClient realtime = new();
            ChatViewModel viewModel = new(api, realtime);
            await viewModel.InitializeAsync(conversationId);

            await realtime.RaiseReconnectingAsync();
            viewModel.StatusMessage.Should().Be("Reconectando ao chat...");
            await realtime.RaiseReconnectedAsync();
            viewModel.StatusMessage.Should().BeEmpty();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(originalContext);
        }
    }

    [Fact]
    public async Task OlderMessagesShouldBePrependedWithoutDuplicates()
    {
        Guid conversationId = Guid.NewGuid();
        ChatMessageModel recent = new(Guid.NewGuid(), conversationId, Guid.NewGuid(), "Recente", DateTimeOffset.UtcNow);
        ChatMessageModel older = new(Guid.NewGuid(), conversationId, Guid.NewGuid(), "Antiga", DateTimeOffset.UtcNow.AddMinutes(-1));
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        api.GetMessagesAsync(conversationId, 1, Arg.Any<CancellationToken>()).Returns(new ChatMessagePageModel([recent], 1, 30, true));
        api.GetMessagesAsync(conversationId, 2, Arg.Any<CancellationToken>()).Returns(new ChatMessagePageModel([older], 2, 30, false));
        ChatViewModel viewModel = new(api, new RealtimeClient());

        await viewModel.InitializeAsync(conversationId);
        await viewModel.LoadOlderCommand.ExecuteAsync(null);

        viewModel.Messages.Select(x => x.Content).Should().ContainInOrder("Antiga", "Recente");
    }

    private sealed class RealtimeClient : IChatRealtimeClient
    {
        public event Func<ChatMessageModel, Task>? MessageReceived;
        public event Func<Task>? Reconnecting;
        public event Func<Task>? Reconnected;
        public Task StartAsync(Guid conversationId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        public Task PublishAsync(ChatMessageModel message) => MessageReceived?.Invoke(message) ?? Task.CompletedTask;
        public Task RaiseReconnectingAsync() => Reconnecting?.Invoke() ?? Task.CompletedTask;
        public Task RaiseReconnectedAsync() => Reconnected?.Invoke() ?? Task.CompletedTask;
    }

    private sealed class ImmediateSynchronizationContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback callback, object? state) => callback(state);
    }
}
