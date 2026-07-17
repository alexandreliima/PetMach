using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Core.Identity;

public sealed partial class RegisterViewModel(IAuthApiClient api, IMobileNavigator navigator) : ObservableObject
{
    private const string CurrentConsentVersion = "2026-07-14";
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private DateTime birthDate = DateTime.Today.AddYears(-18);
    [ObservableProperty] private bool acceptedTerms;
    [ObservableProperty] private bool acceptedPrivacy;
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private bool isBusy;

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (IsBusy) return;
        if (string.IsNullOrWhiteSpace(Email) || Password.Length < 12)
        {
            StatusMessage = "Use um e-mail válido e uma senha com pelo menos 12 caracteres.";
            return;
        }
        if (!AcceptedTerms || !AcceptedPrivacy)
        {
            StatusMessage = "Aceite os termos e a política de privacidade.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Criando sua conta...";
            RegistrationInput input = new(
                Email.Trim(), Password, DateOnly.FromDateTime(BirthDate), true, CurrentConsentVersion, true, CurrentConsentVersion);
            RegistrationResult result = await api.RegisterAsync(input, CancellationToken.None);
            StatusMessage = result.RequiresEmailConfirmation
                ? "Conta criada. Confira a confirmação capturada no ambiente de desenvolvimento."
                : "Conta criada com sucesso.";
            await navigator.GoToAsync($"login?email={Uri.EscapeDataString(input.Email)}&registered={result.RequiresEmailConfirmation.ToString().ToLowerInvariant()}");
        }
        catch (HttpRequestException)
        {
            StatusMessage = "Não foi possível cadastrar. Confira os dados e se a API está ligada.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
