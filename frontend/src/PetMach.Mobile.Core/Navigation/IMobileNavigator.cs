namespace PetMach.Mobile.Core.Navigation;

public interface IMobileNavigator
{
    Task GoToAsync(string route);
}
