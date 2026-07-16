using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile;

public partial class NotificationsPage : ContentPage
{
    private readonly NotificationsViewModel viewModel;

    public NotificationsPage(NotificationsViewModel viewModel)
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
