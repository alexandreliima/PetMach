using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile;

public partial class AdoptionPage : ContentPage
{
    private readonly AdoptionViewModel viewModel;
    public AdoptionPage(AdoptionViewModel viewModel) { InitializeComponent(); this.viewModel = viewModel; BindingContext = viewModel; }
    protected override void OnAppearing() { base.OnAppearing(); viewModel.LoadCommand.Execute(null); }
}
