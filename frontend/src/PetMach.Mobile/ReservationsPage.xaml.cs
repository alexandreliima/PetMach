using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile;

public partial class ReservationsPage : ContentPage
{
    private readonly ReservationsViewModel viewModel;
    public ReservationsPage(ReservationsViewModel viewModel) { InitializeComponent(); this.viewModel = viewModel; BindingContext = viewModel; }
    protected override void OnAppearing() { base.OnAppearing(); viewModel.LoadCommand.Execute(null); }
}
