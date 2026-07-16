using PetMach.Mobile.Core.Identity;

namespace PetMach.Mobile;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
