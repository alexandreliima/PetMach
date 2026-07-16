using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PetMach.Mobile.Core.Features;

public sealed partial class ChatViewModel(IPetMachApiClient api, IChatRealtimeClient realtime) : ObservableObject
{
    public ObservableCollection<ChatMessageModel> Messages { get; } = [];
    [ObservableProperty] private string draft = string.Empty;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = string.Empty;
    private Guid conversationId;
    private int currentPage;
    private bool hasMore;
    private SynchronizationContext? uiContext;

    public async Task InitializeAsync(Guid id)
    {
        uiContext = SynchronizationContext.Current;
        realtime.MessageReceived -= OnMessageReceivedAsync;
        realtime.Reconnecting -= OnReconnectingAsync;
        realtime.Reconnected -= OnReconnectedAsync;
        realtime.MessageReceived += OnMessageReceivedAsync;
        realtime.Reconnecting += OnReconnectingAsync;
        realtime.Reconnected += OnReconnectedAsync;
        conversationId = id;
        Messages.Clear();
        currentPage = 0;
        await LoadPageAsync(1);
        try { await realtime.StartAsync(id, CancellationToken.None); }
        catch (HttpRequestException) { StatusMessage = "Histórico carregado. Tempo real indisponível; tente novamente ao reabrir."; }
    }

    public Task StopAsync() => realtime.StopAsync(CancellationToken.None);

    [RelayCommand]
    private async Task LoadOlderAsync()
    {
        if (!hasMore || IsBusy) return;
        await LoadPageAsync(currentPage + 1);
    }

    [RelayCommand]
    private async Task SendAsync()
    {
        if (conversationId == Guid.Empty || string.IsNullOrWhiteSpace(Draft) || Draft.Trim().Length > 2000 || IsBusy) return;
        try
        {
            IsBusy = true;
            ChatMessageModel message = await api.SendMessageAsync(conversationId, Draft.Trim(), CancellationToken.None);
            AddIfMissing(message);
            Draft = string.Empty;
        }
        catch (HttpRequestException) { StatusMessage = "Não foi possível enviar a mensagem."; }
        finally { IsBusy = false; }
    }

    private async Task LoadPageAsync(int page)
    {
        try
        {
            IsBusy = true;
            ChatMessagePageModel result = await api.GetMessagesAsync(conversationId, page, CancellationToken.None);
            foreach (ChatMessageModel message in result.Items.Reverse())
                if (!Messages.Any(existing => existing.Id == message.Id)) Messages.Insert(0, message);
            if (Messages.LastOrDefault() is ChatMessageModel latest)
                _ = await api.MarkConversationReadAsync(conversationId, latest.Id, CancellationToken.None);
            currentPage = result.Page;
            hasMore = result.HasMore;
            StatusMessage = Messages.Count == 0 ? "Comece a conversa com uma mensagem." : string.Empty;
        }
        catch (AuthenticationRequiredException ex) { StatusMessage = ex.Message; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível carregar as mensagens."; }
        finally { IsBusy = false; }
    }

    private Task OnMessageReceivedAsync(ChatMessageModel message)
    {
        if (message.ConversationId != conversationId) return Task.CompletedTask;
        if (uiContext is null) AddIfMissing(message);
        else uiContext.Post(_ => AddIfMissing(message), null);
        return MarkReceivedAsReadAsync(message);
    }

    private Task OnReconnectingAsync()
    {
        SetStatus("Reconectando ao chat...");
        return Task.CompletedTask;
    }

    private Task OnReconnectedAsync()
    {
        SetStatus(string.Empty);
        return Task.CompletedTask;
    }

    private void AddIfMissing(ChatMessageModel message)
    {
        if (!Messages.Any(existing => existing.Id == message.Id)) Messages.Add(message);
    }

    private void SetStatus(string value)
    {
        if (uiContext is null) StatusMessage = value;
        else uiContext.Post(_ => StatusMessage = value, null);
    }

    private async Task MarkReceivedAsReadAsync(ChatMessageModel message)
    {
        try { _ = await api.MarkConversationReadAsync(conversationId, message.Id, CancellationToken.None); }
        catch (HttpRequestException) { SetStatus("Mensagem recebida, mas a confirmação de leitura falhou."); }
    }
}
