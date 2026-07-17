using PetMach.Mobile.Core.Identity;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile;

public partial class LoginPage : ContentPage, IQueryAttributable
{
    private readonly LoginViewModel viewModel;
    private readonly IMobileNavigator navigator;

    public LoginPage(LoginViewModel viewModel, IMobileNavigator navigator)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        this.navigator = navigator;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("email", out object? email)) viewModel.Email = email?.ToString() ?? string.Empty;
        if (query.TryGetValue("registered", out object? registered) &&
            bool.TryParse(registered?.ToString(), out bool requiresConfirmation) &&
            requiresConfirmation)
            viewModel.StatusMessage = "Conta criada. Confirme o e-mail antes de entrar.";
    }

    private async void OpenRegistrationClicked(object sender, EventArgs e)
    {
        await navigator.GoToAsync("register");
    }
}
