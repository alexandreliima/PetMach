using PetMach.Mobile.Core.Identity;

namespace PetMach.Mobile;

public partial class MainPage : ContentPage
{
    private bool hasAdvanced;

    public MainPage(WelcomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (hasAdvanced) return;

        await Task.Delay(2200);
        ShowOnboarding();
    }

    private void AdvanceTapped(object sender, TappedEventArgs e)
    {
        ShowOnboarding();
    }

    private void ShowOnboarding()
    {
        if (hasAdvanced) return;

        hasAdvanced = true;
        SplashContent.IsVisible = false;
        OnboardingContent.IsVisible = true;
    }
}
