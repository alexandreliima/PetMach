using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile;

public partial class TutorProfilePage : ContentPage
{
    private readonly TutorProfileViewModel viewModel;

    public TutorProfilePage(TutorProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.LoadCommand.ExecuteAsync(null);
    }
}
