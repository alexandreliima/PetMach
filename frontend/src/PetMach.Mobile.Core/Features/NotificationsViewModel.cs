using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PetMach.Mobile.Core.Features;

public sealed partial class NotificationsViewModel(IPetMachApiClient api) : ObservableObject
{
    public ObservableCollection<NotificationModel> Notifications { get; } = [];
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = string.Empty;

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            Notifications.Clear();
            foreach (NotificationModel notification in await api.GetNotificationsAsync(CancellationToken.None))
                Notifications.Add(notification);
            StatusMessage = Notifications.Count == 0 ? "Nenhuma notificação por enquanto." : string.Empty;
        }
        catch (AuthenticationRequiredException ex) { StatusMessage = ex.Message; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível carregar as notificações."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task MarkAsReadAsync(NotificationModel? notification)
    {
        if (notification is null || notification.ReadAtUtc.HasValue || IsBusy) return;
        try
        {
            IsBusy = true;
            await api.MarkNotificationAsReadAsync(notification.Id, CancellationToken.None);
            int index = Notifications.IndexOf(notification);
            Notifications[index] = notification with { ReadAtUtc = DateTimeOffset.UtcNow };
        }
        catch (HttpRequestException) { StatusMessage = "Não foi possível marcar a notificação como lida."; }
        finally { IsBusy = false; }
    }
}
