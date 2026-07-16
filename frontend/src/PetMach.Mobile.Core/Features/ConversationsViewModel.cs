using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Core.Features;

public sealed partial class ConversationsViewModel(IPetMachApiClient api, IMobileNavigator navigator) : ObservableObject
{
    public ObservableCollection<ConversationModel> Conversations { get; } = [];
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = string.Empty;

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            Conversations.Clear();
            foreach (ConversationModel conversation in await api.GetConversationsAsync(CancellationToken.None)) Conversations.Add(conversation);
            StatusMessage = Conversations.Count == 0 ? "Nenhuma conversa ativa." : string.Empty;
        }
        catch (AuthenticationRequiredException ex) { StatusMessage = ex.Message; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível carregar as conversas."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private Task OpenAsync(ConversationModel? conversation) => conversation is null
        ? Task.CompletedTask
        : navigator.GoToAsync($"chat?conversationId={conversation.Id}");
}
