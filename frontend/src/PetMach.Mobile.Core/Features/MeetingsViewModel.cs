using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PetMach.Mobile.Core.Features;

public sealed partial class MeetingsViewModel(IPetMachApiClient api) : ObservableObject
{
    public ObservableCollection<MatchModel> Matches { get; } = [];
    public ObservableCollection<MeetingModel> Meetings { get; } = [];
    private Guid? requestedMatchId;
    [ObservableProperty] private MatchModel? selectedMatch;
    [ObservableProperty] private DateTime scheduledDate = DateTime.Today.AddDays(1);
    [ObservableProperty] private TimeSpan scheduledTime = new(10, 0, 0);
    [ObservableProperty] private string placeName = string.Empty;
    [ObservableProperty] private string notes = string.Empty;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = string.Empty;

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            Matches.Clear();
            Meetings.Clear();
            foreach (MatchModel match in await api.GetMatchesAsync(CancellationToken.None)) Matches.Add(match);
            foreach (MeetingModel meeting in await api.GetMeetingsAsync(CancellationToken.None)) Meetings.Add(meeting);
            SelectedMatch = requestedMatchId.HasValue
                ? Matches.FirstOrDefault(match => match.Id == requestedMatchId.Value)
                : SelectedMatch ?? Matches.FirstOrDefault();
        }
        catch (AuthenticationRequiredException ex) { StatusMessage = ex.Message; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível carregar os encontros."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ProposeAsync()
    {
        if (SelectedMatch is null || string.IsNullOrWhiteSpace(PlaceName) || IsBusy) return;
        DateTime local = ScheduledDate.Date.Add(ScheduledTime);
        DateTimeOffset scheduled = new(local, TimeZoneInfo.Local.GetUtcOffset(local));
        if (scheduled <= DateTimeOffset.Now) { StatusMessage = "Escolha uma data e hora futuras."; return; }
        try
        {
            IsBusy = true;
            MeetingModel meeting = await api.CreateMeetingAsync(SelectedMatch.Id, scheduled.ToUniversalTime(), PlaceName.Trim(), Clean(Notes), CancellationToken.None);
            Meetings.Insert(0, meeting);
            PlaceName = string.Empty;
            Notes = string.Empty;
            StatusMessage = "Encontro proposto.";
        }
        catch (HttpRequestException) { StatusMessage = "Não foi possível propor o encontro."; }
        finally { IsBusy = false; }
    }

    [RelayCommand] private Task AcceptAsync(MeetingModel meeting) => TransitionAsync(meeting, "accept");
    [RelayCommand] private Task DeclineAsync(MeetingModel meeting) => TransitionAsync(meeting, "decline");
    [RelayCommand] private Task CancelAsync(MeetingModel meeting) => TransitionAsync(meeting, "cancel");

    public void SelectMatch(Guid matchId)
    {
        requestedMatchId = matchId;
        SelectedMatch = Matches.FirstOrDefault(match => match.Id == matchId);
    }

    private async Task TransitionAsync(MeetingModel meeting, string transition)
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            MeetingModel updated = await api.TransitionMeetingAsync(meeting.Id, transition, CancellationToken.None);
            Meetings[Meetings.IndexOf(meeting)] = updated;
        }
        catch (HttpRequestException) { StatusMessage = "Não foi possível atualizar o encontro."; }
        finally { IsBusy = false; }
    }

    private static string? Clean(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
