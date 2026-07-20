using CommunityToolkit.Mvvm.Input;
using PetMach.Mobile.Core.Identity;
using PetMach.Mobile.Core.Navigation;
using PetMach.Mobile.Core.Settings;

namespace PetMach.Mobile.Core.Home;

public sealed partial class HomeViewModel(
    IMobileNavigator navigator,
    ILogoutCoordinator logoutCoordinator)
{
    [RelayCommand]
    private Task OpenTutorProfileAsync() => navigator.GoToAsync("tutor-profile");

    [RelayCommand]
    private Task OpenDogsAsync() => navigator.GoToAsync("dogs");

    [RelayCommand]
    private Task OpenHealthAsync() => navigator.GoToAsync("health");

    [RelayCommand]
    private Task OpenDiscoveryAsync() => navigator.GoToAsync("discovery");

    [RelayCommand]
    private Task OpenMatchesAsync() => navigator.GoToAsync("matches");

    [RelayCommand]
    private Task OpenNotificationsAsync() => navigator.GoToAsync("notifications");

    [RelayCommand]
    private Task OpenConversationsAsync() => navigator.GoToAsync("conversations");

    [RelayCommand]
    private Task OpenMeetingsAsync() => navigator.GoToAsync("meetings");

    [RelayCommand]
    private Task OpenPartnerSpacesAsync() => navigator.GoToAsync("partner-spaces");

    [RelayCommand]
    private Task OpenReservationsAsync() => navigator.GoToAsync("reservations");

    [RelayCommand]
    private Task OpenPartnerOperationsAsync() => navigator.GoToAsync("partner-operations");

    [RelayCommand]
    private Task OpenAdoptionAsync() => navigator.GoToAsync("adoption");

    [RelayCommand]
    private Task OpenSettingsAsync() => navigator.GoToAsync(SettingsRoutes.Settings);

    [RelayCommand]
    private Task LogoutAsync(CancellationToken cancellationToken) =>
        logoutCoordinator.LogoutAsync(cancellationToken);
}
