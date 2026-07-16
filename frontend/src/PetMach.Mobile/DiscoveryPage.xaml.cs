using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile;

public partial class DiscoveryPage : ContentPage
{
    private readonly DiscoveryViewModel viewModel;
    public DiscoveryPage(DiscoveryViewModel viewModel)
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
