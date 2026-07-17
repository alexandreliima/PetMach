using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Core.Identity;

public sealed partial class LoginViewModel(AuthenticationSession session, IMobileNavigator navigator) : ObservableObject
{
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private bool isBusy;

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy) return;
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "Informe o e-mail e a senha.";
            return;
        }
        try
        {
            IsBusy = true;
            StatusMessage = "Entrando...";
            await session.LoginAsync(Email.Trim(), Password, CancellationToken.None);
            StatusMessage = string.Empty;
            await navigator.GoToAsync("//app/network");
        }
        catch (AuthenticationApiException ex) when (ex.Code == "identity.email_not_confirmed")
        {
            StatusMessage = "Confirme seu e-mail antes de entrar. No ambiente local, consulte a pasta .dev-emails da API.";
        }
        catch (AuthenticationApiException ex)
        {
            StatusMessage = ex.Message;
        }
        catch (HttpRequestException)
        {
            StatusMessage = "Não foi possível conectar à API.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
