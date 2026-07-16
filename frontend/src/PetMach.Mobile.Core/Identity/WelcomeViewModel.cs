using CommunityToolkit.Mvvm.Input;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Core.Identity;

public sealed partial class WelcomeViewModel(IMobileNavigator navigator)
{
    [RelayCommand]
    private Task OpenLoginAsync() => navigator.GoToAsync("login");

    [RelayCommand]
    private Task OpenRegistrationAsync() => navigator.GoToAsync("register");

    [RelayCommand]
    private Task OpenAboutAsync() => navigator.GoToAsync("about");
}
