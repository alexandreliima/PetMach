using PetMach.Mobile.Core.Identity;

namespace PetMach.Mobile;

public partial class MainPage : ContentPage
{
    private static readonly IReadOnlyList<OnboardingItem> Items =
    [
        new(
            "Encontre o melhor para seu pet",
            "Descubra novas amizades, experiências e cuidados pensados para o bem-estar do seu companheiro.",
            "petmach_welcome_dogs.png",
            "Ilustração de dois cães felizes",
            Aspect.AspectFit,
            "Cuidado",
            "Passeios",
            "Bem-estar"),
        new(
            "Conexões que fazem sentido",
            "Crie matches entre pets, conheça histórias de adoção responsável e reserve serviços em poucos passos.",
            "petmatch_onboarding_dogs_android.jpg",
            "Cães brincando juntos em um parque",
            Aspect.AspectFill,
            "Match",
            "Adoção",
            "Reservas"),
        new(
            "Uma comunidade em que confiar",
            "Conte com segurança, espaços parceiros e uma comunidade que compartilha o mesmo cuidado pelos animais.",
            "petmatch_logo.svg",
            "Símbolo PetMatch",
            Aspect.AspectFit,
            "Segurança",
            "Parceiros",
            "Comunidade"),
    ];

    private bool hasAdvanced;

    public MainPage(WelcomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public IReadOnlyList<OnboardingItem> OnboardingItems { get; } = Items;

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (hasAdvanced)
        {
            return;
        }

        SplashBrand.Opacity = 0;
        SplashBrand.Scale = 0.92;
        SplashIndicator.Opacity = 0;

        await Task.WhenAll(
            SplashBrand.FadeToAsync(1, 500, Easing.CubicOut),
            SplashBrand.ScaleToAsync(1, 600, Easing.CubicOut));
        await SplashIndicator.FadeToAsync(1, 250, Easing.CubicOut);
        await Task.Delay(1150);
        await ShowOnboardingAsync();
    }

    private async void AdvanceTapped(object sender, TappedEventArgs e)
    {
        await ShowOnboardingAsync();
    }

    private async Task ShowOnboardingAsync()
    {
        if (hasAdvanced)
        {
            return;
        }

        hasAdvanced = true;
        await SplashContent.FadeToAsync(0, 220, Easing.CubicIn);
        SplashContent.IsVisible = false;
        OnboardingContent.IsVisible = true;
        await OnboardingContent.FadeToAsync(1, 320, Easing.CubicOut);
    }

    private void NextClicked(object sender, EventArgs e)
    {
        if (OnboardingCarousel.Position < Items.Count - 1)
        {
            OnboardingCarousel.Position++;
        }
    }

    private void OnboardingPositionChanged(object sender, PositionChangedEventArgs e)
    {
        bool isLastPage = e.CurrentPosition == Items.Count - 1;
        NextButton.IsVisible = !isLastPage;
        StartButton.IsVisible = isLastPage;
    }

    public sealed record OnboardingItem(
        string Title,
        string Description,
        string ImageSource,
        string ImageDescription,
        Aspect ImageAspect,
        string FeatureOne,
        string FeatureTwo,
        string FeatureThree);
}
