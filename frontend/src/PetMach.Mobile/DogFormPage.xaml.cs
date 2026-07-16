using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile;

public partial class DogFormPage : ContentPage
{
    public DogFormPage(DogFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
