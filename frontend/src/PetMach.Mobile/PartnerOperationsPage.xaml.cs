using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile;

public partial class PartnerOperationsPage : ContentPage
{
    private readonly PartnerOperationsViewModel viewModel;
    public PartnerOperationsPage(PartnerOperationsViewModel viewModel) { InitializeComponent(); this.viewModel = viewModel; BindingContext = viewModel; }
    protected override void OnAppearing() { base.OnAppearing(); viewModel.LoadCommand.Execute(null); }
}
