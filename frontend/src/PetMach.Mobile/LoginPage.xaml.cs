using PetMach.Mobile.Core.Identity;
using PetMach.Mobile.Core.Navigation;
using PetMach.Mobile.Components;

namespace PetMach.Mobile;

public partial class LoginPage : ContentPage, IQueryAttributable
{
    private readonly LoginViewModel viewModel;
    private readonly IMobileNavigator navigator;
    private bool hasAnimated;

    public LoginPage(LoginViewModel viewModel, IMobileNavigator navigator)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        this.navigator = navigator;
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (hasAnimated)
        {
            return;
        }

        hasAnimated = true;
        LoginContent.Opacity = 0;
        LoginContent.TranslationY = 16;
        await Task.WhenAll(
            LoginContent.FadeToAsync(1, 320, Easing.CubicOut),
            LoginContent.TranslateToAsync(0, 0, 360, Easing.CubicOut));
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

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(LoginViewModel.StatusMessage) or nameof(LoginViewModel.IsBusy))
        {
            UpdateFeedback();
        }
    }

    private void UpdateFeedback()
    {
        string message = viewModel.StatusMessage;
        bool isVisible = !viewModel.IsBusy && !string.IsNullOrWhiteSpace(message);
        bool isSuccess = message.StartsWith("Conta criada.", StringComparison.Ordinal);

        LoginFeedback.IsVisible = isVisible;
        LoginFeedback.Kind = isSuccess ? StateViewKind.Success : StateViewKind.Error;
        LoginFeedback.Title = isSuccess ? "Conta criada" : "Não foi possível entrar";
    }
}
