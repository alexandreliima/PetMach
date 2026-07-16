using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile;

public partial class HealthPage : ContentPage
{
    private readonly HealthViewModel viewModel;

    public HealthPage(HealthViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.LoadDogsCommand.ExecuteAsync(null);
    }
}
