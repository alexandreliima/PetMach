using PetMach.Mobile.Core.Identity;

namespace PetMach.Mobile;

public partial class OnboardingPage : ContentPage
{
    public OnboardingPage(WelcomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
