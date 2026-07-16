using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Navigation;

public sealed class ShellNavigator : IMobileNavigator
{
    public Task GoToAsync(string route) => Shell.Current.GoToAsync(route);
}
