using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile;

public partial class ConversationsPage : ContentPage
{
    private readonly ConversationsViewModel viewModel;
    public ConversationsPage(ConversationsViewModel viewModel)
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
