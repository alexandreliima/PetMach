using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PetMach.Mobile.Core.Identity;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Core.Settings;

public sealed partial class SettingsViewModel(
    IMobileNavigator navigator,
    IConfirmationService confirmation,
    ILogoutCoordinator logoutCoordinator) : ObservableObject
{
    private int logoutInProgress;

    [ObservableProperty]
    private IReadOnlyList<SettingsSection> sections = [];

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isLoggingOut;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool ShowContent => !IsLoading;
    public string LogoutButtonText => IsLoggingOut ? "Saindo..." : "Sair da conta";

    partial void OnIsLoadingChanged(bool value) => OnPropertyChanged(nameof(ShowContent));

    partial void OnIsLoggingOutChanged(bool value)
    {
        OnPropertyChanged(nameof(LogoutButtonText));
        OpenItemCommand.NotifyCanExecuteChanged();
    }

    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));

    [RelayCommand]
    private Task LoadAsync(CancellationToken cancellationToken)
    {
        if (IsLoading || Sections.Count > 0)
        {
            return Task.CompletedTask;
        }

        IsLoading = true;
        try
        {
            ErrorMessage = string.Empty;
            cancellationToken.ThrowIfCancellationRequested();
            Sections = CreateSections();
            return Task.CompletedTask;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpenItem))]
    private async Task OpenItemAsync(
        SettingsSectionItem? item,
        CancellationToken cancellationToken)
    {
        if (!CanOpenItem(item) || item?.Route is not string route)
        {
            return;
        }

        try
        {
            ErrorMessage = string.Empty;
            cancellationToken.ThrowIfCancellationRequested();
            await navigator.GoToAsync(route);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            ErrorMessage = "Não foi possível abrir esta opção. Tente novamente.";
        }
    }

    private bool CanOpenItem(SettingsSectionItem? item) =>
        item is
        {
            IsEnabled: true,
            Route: not null,
        } &&
        !string.IsNullOrWhiteSpace(item.Route) &&
        !IsLoggingOut;

    [RelayCommand]
    private async Task LogoutAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref logoutInProgress, 1) != 0)
        {
            return;
        }

        try
        {
            IsLoggingOut = true;
            ErrorMessage = string.Empty;
            bool confirmed = await confirmation.ConfirmAsync(
                new ConfirmationRequest(
                    "Sair da conta",
                    "Deseja sair da sua conta? Será necessário entrar novamente para acessar o PetMatch.",
                    "Sair",
                    "Cancelar"),
                cancellationToken);
            if (!confirmed)
            {
                return;
            }

            await logoutCoordinator.LogoutAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            ErrorMessage = "Não foi possível sair da conta. Tente novamente.";
        }
        finally
        {
            IsLoggingOut = false;
            Interlocked.Exchange(ref logoutInProgress, 0);
        }
    }

    private static IReadOnlyList<SettingsSection> CreateSections() =>
    [
        new(
            "Conta",
            [
                ComingSoon(
                    "account",
                    "Minha Conta",
                    "Consulte e edite seus dados pessoais."),
            ]),
        new(
            "Preferências",
            [
                ComingSoon(
                    "appearance",
                    "Aparência",
                    "Escolha como o PetMatch aparece neste dispositivo."),
                ComingSoon(
                    "notifications",
                    "Notificações",
                    "Controle os avisos que deseja receber."),
            ]),
        new(
            "Privacidade e segurança",
            [
                ComingSoon(
                    "privacy",
                    "Privacidade",
                    "Gerencie como suas informações são apresentadas."),
                ComingSoon(
                    "security",
                    "Segurança",
                    "Proteja sua conta e revise opções de acesso."),
            ]),
        new(
            "Informações",
            [
                new(
                    "about",
                    "Sobre o PetMatch",
                    "Conheça o aplicativo e consulte sua versão.",
                    SettingsRoutes.AboutFromSettings,
                    true),
                ComingSoon(
                    "terms",
                    "Termos de Uso",
                    "Consulte as condições de uso do PetMatch."),
                ComingSoon(
                    "privacy-policy",
                    "Política de Privacidade",
                    "Entenda como seus dados serão tratados."),
                ComingSoon(
                    "licenses",
                    "Licenças Open Source",
                    "Consulte os componentes de código aberto utilizados."),
            ]),
    ];

    private static SettingsSectionItem ComingSoon(
        string id,
        string title,
        string description) =>
        new(id, title, description, null, false, "Em breve");
}
