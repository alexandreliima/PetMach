using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PetMach.Mobile.Core.Features;

public sealed partial class PartnerSpacesViewModel(IPetMachApiClient api) : ObservableObject
{
    public ObservableCollection<PartnerSpaceModel> Spaces { get; } = [];
    public ObservableCollection<SpaceAvailabilityModel> Availability { get; } = [];
    [ObservableProperty] private string city = string.Empty;
    [ObservableProperty] private string state = string.Empty;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private PartnerSpaceModel? selectedSpace;

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            Spaces.Clear();
            foreach (PartnerSpaceModel space in await api.GetPartnerSpacesAsync(Clean(City), Clean(State), CancellationToken.None)) Spaces.Add(space);
            StatusMessage = Spaces.Count == 0 ? "Nenhum espaço encontrado." : string.Empty;
        }
        catch (AuthenticationRequiredException ex) { StatusMessage = ex.Message; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível carregar os espaços."; }
        finally { IsBusy = false; }
    }

    partial void OnSelectedSpaceChanged(PartnerSpaceModel? value)
    {
        Availability.Clear();
        if (value is not null) LoadAvailabilityCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadAvailabilityAsync()
    {
        if (SelectedSpace is null) return;
        try
        {
            Availability.Clear();
            foreach (SpaceAvailabilityModel period in await api.GetSpaceAvailabilityAsync(SelectedSpace.Id, CancellationToken.None)) Availability.Add(period);
            StatusMessage = Availability.Count == 0 ? "Nenhum horário disponível nos próximos 30 dias." : string.Empty;
        }
        catch (AuthenticationRequiredException ex) { StatusMessage = ex.Message; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível carregar a disponibilidade."; }
    }

    private static string? Clean(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
