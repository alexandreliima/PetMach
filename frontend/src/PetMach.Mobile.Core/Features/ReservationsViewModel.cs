using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PetMach.Mobile.Core.Features;

public sealed partial class ReservationsViewModel(IPetMachApiClient api) : ObservableObject
{
    public ObservableCollection<DogModel> Dogs { get; } = [];
    public ObservableCollection<PartnerSpaceModel> Spaces { get; } = [];
    public ObservableCollection<SpaceAvailabilityModel> Availability { get; } = [];
    public ObservableCollection<ReservationModel> Reservations { get; } = [];
    [ObservableProperty] private DogModel? selectedDog;
    [ObservableProperty] private PartnerSpaceModel? selectedSpace;
    [ObservableProperty] private SpaceAvailabilityModel? selectedAvailability;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = string.Empty;

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true; StatusMessage = string.Empty;
            Dogs.Clear(); Spaces.Clear(); Reservations.Clear(); Availability.Clear();
            foreach (DogModel dog in await api.GetDogsAsync(CancellationToken.None)) Dogs.Add(dog);
            foreach (PartnerSpaceModel space in await api.GetPartnerSpacesAsync(null, null, CancellationToken.None)) Spaces.Add(space);
            foreach (ReservationModel reservation in await api.GetReservationsAsync(CancellationToken.None)) Reservations.Add(reservation);
            StatusMessage = Reservations.Count == 0 ? "Você ainda não possui reservas." : string.Empty;
        }
        catch (AuthenticationRequiredException ex) { StatusMessage = ex.Message; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível carregar as reservas."; }
        finally { IsBusy = false; }
    }

    partial void OnSelectedSpaceChanged(PartnerSpaceModel? value)
    {
        Availability.Clear(); SelectedAvailability = null;
        if (value is not null) LoadAvailabilityCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadAvailabilityAsync()
    {
        if (SelectedSpace is null) return;
        try { foreach (SpaceAvailabilityModel period in await api.GetSpaceAvailabilityAsync(SelectedSpace.Id, CancellationToken.None)) Availability.Add(period); }
        catch (HttpRequestException) { StatusMessage = "Não foi possível carregar os horários."; }
    }

    [RelayCommand]
    private async Task CreateAsync()
    {
        if (SelectedDog is null || SelectedAvailability is null) { StatusMessage = "Selecione um cão e um horário."; return; }
        try
        {
            await api.CreateReservationAsync(SelectedAvailability.Id, SelectedDog.Id, CancellationToken.None);
            await LoadAsync();
            StatusMessage = "Reserva solicitada. Aguarde a confirmação do parceiro.";
        }
        catch (HttpRequestException) { StatusMessage = "O horário não está mais disponível."; }
    }

    [RelayCommand]
    private async Task CancelAsync(ReservationModel? reservation)
    {
        if (reservation is null || !reservation.CanCancel) return;
        try { await api.CancelReservationAsync(reservation.Id, CancellationToken.None); await LoadAsync(); StatusMessage = "Reserva cancelada."; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível cancelar a reserva."; }
    }
}
