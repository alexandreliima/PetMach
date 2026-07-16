using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile;

public partial class DogsPage : ContentPage
{
    private readonly DogsViewModel viewModel;

    public DogsPage(DogsViewModel viewModel)
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
