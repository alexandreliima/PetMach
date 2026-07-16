using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile;

public partial class MatchesPage : ContentPage
{
    private readonly MatchesViewModel viewModel;
    public MatchesPage(MatchesViewModel viewModel)
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
