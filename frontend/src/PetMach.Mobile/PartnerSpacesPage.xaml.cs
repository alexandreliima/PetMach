using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile;

public partial class PartnerSpacesPage : ContentPage
{
    private readonly PartnerSpacesViewModel viewModel;
    public PartnerSpacesPage(PartnerSpacesViewModel viewModel)
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
