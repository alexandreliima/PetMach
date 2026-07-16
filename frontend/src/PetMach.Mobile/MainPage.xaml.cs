using PetMach.Mobile.Core.Identity;

namespace PetMach.Mobile;

public partial class MainPage : ContentPage
{
    public MainPage(WelcomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
