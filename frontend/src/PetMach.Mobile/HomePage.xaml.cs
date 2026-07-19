using PetMach.Mobile.Core.Home;
using PetMach.Mobile.Presentation;

namespace PetMach.Mobile;

public partial class HomePage : ContentPage
{
    private bool hasLoaded;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public HomeExperienceState Presentation { get; } = new();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (hasLoaded)
        {
            return;
        }

        await LoadPresentationAsync();
    }

    private async void RetryClicked(object sender, EventArgs e)
    {
        await LoadPresentationAsync();
    }

    private async Task LoadPresentationAsync()
    {
        Presentation.BeginLoading();
        await Task.Delay(350);

        Presentation.LoadDemo();
        hasLoaded = true;
        HomeContent.Opacity = 0;
        HomeContent.TranslationY = 12;
        await Task.WhenAll(
            HomeContent.FadeToAsync(1, 280, Easing.CubicOut),
            HomeContent.TranslateToAsync(0, 0, 320, Easing.CubicOut));
    }
}
