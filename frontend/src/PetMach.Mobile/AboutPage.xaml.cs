using PetMach.Mobile.Core.Identity;

namespace PetMach.Mobile;

public partial class AboutPage : ContentPage
{
    public AboutPage(WelcomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
