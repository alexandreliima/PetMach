using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile;

public partial class MeetingsPage : ContentPage, IQueryAttributable
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

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("matchId", out object? value) &&
            Guid.TryParse(value?.ToString(), out Guid matchId))
        {
            viewModel.SelectMatch(matchId);
        }
    }

    private async void OpenPartnerReservationClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("reservations");
    }
}
