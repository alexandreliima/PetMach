using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile;

public partial class MeetingsPage : ContentPage
{
    private readonly MeetingsViewModel viewModel;
    public MeetingsPage(MeetingsViewModel viewModel)
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
