using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PetMach.Mobile.Core.Features;

public sealed partial class MatchesViewModel(IPetMachApiClient api) : ObservableObject
{
    public ObservableCollection<MatchModel> Matches { get; } = [];
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
            foreach (MatchModel match in await api.GetMatchesAsync(CancellationToken.None)) Matches.Add(match);
            StatusMessage = Matches.Count == 0 ? "Seus matches aparecerão aqui." : string.Empty;
        }
        catch (AuthenticationRequiredException ex) { StatusMessage = ex.Message; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível carregar os matches."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task EndAsync(MatchModel match)
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            await api.EndMatchAsync(match.Id, CancellationToken.None);
            Matches.Remove(match);
            StatusMessage = "Match desfeito.";
        }
        catch (HttpRequestException) { StatusMessage = "Não foi possível desfazer o match."; }
        finally { IsBusy = false; }
    }
}
