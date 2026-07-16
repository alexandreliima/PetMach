using PetMach.Mobile.Core.Home;

namespace PetMach.Mobile;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
