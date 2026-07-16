using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PetMach.Mobile.Core.Features;

public sealed partial class PartnerOperationsViewModel(IPetMachApiClient api, TimeProvider timeProvider) : ObservableObject
{
    public ObservableCollection<PartnerSpaceModel> Spaces { get; } = [];
    public ObservableCollection<ReservationModel> Reservations { get; } = [];
    [ObservableProperty] private PartnerManagementModel? partner;
    [ObservableProperty] private PartnerSpaceModel? selectedSpace;
    [ObservableProperty] private DateTime availabilityDate = DateTime.Today.AddDays(1);
    [ObservableProperty] private TimeSpan startsAt = new(9, 0, 0);
    [ObservableProperty] private TimeSpan endsAt = new(10, 0, 0);
    [ObservableProperty] private bool paymentReceivedOnSite;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = string.Empty;

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true; StatusMessage = string.Empty; Spaces.Clear(); Reservations.Clear();
            Partner = await api.GetManagedPartnerAsync(CancellationToken.None);
            foreach (PartnerSpaceModel space in await api.GetManagedPartnerSpacesAsync(CancellationToken.None)) Spaces.Add(space);
            foreach (ReservationModel reservation in await api.GetPartnerReservationsAsync(CancellationToken.None)) Reservations.Add(reservation);
        }
        catch (AuthenticationRequiredException ex) { StatusMessage = ex.Message; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível carregar o painel do parceiro."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task CreateAvailabilityAsync()
    {
        if (Partner is null || SelectedSpace is null) { StatusMessage = "Selecione um espaço."; return; }
        try
        {
            TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(Partner.TimeZoneId);
            DateTime localStart = DateTime.SpecifyKind(AvailabilityDate.Date + StartsAt, DateTimeKind.Unspecified);
            DateTime localEnd = DateTime.SpecifyKind(AvailabilityDate.Date + EndsAt, DateTimeKind.Unspecified);
            DateTimeOffset startUtc = new(TimeZoneInfo.ConvertTimeToUtc(localStart, zone), TimeSpan.Zero);
            DateTimeOffset endUtc = new(TimeZoneInfo.ConvertTimeToUtc(localEnd, zone), TimeSpan.Zero);
            if (startUtc <= timeProvider.GetUtcNow() || endUtc <= startUtc) { StatusMessage = "Informe um período futuro válido."; return; }
            await api.CreateSpaceAvailabilityAsync(SelectedSpace.Id, startUtc, endUtc, CancellationToken.None);
            StatusMessage = "Disponibilidade criada.";
        }
        catch (ArgumentException) { StatusMessage = "O horário não existe ou é ambíguo no fuso do estabelecimento."; }
        catch (TimeZoneNotFoundException) { StatusMessage = "Fuso do estabelecimento inválido."; }
        catch (HttpRequestException) { StatusMessage = "O período é inválido ou conflita com outro horário."; }
    }

    [RelayCommand] private Task ConfirmAsync(ReservationModel reservation) => TransitionAsync(reservation, "confirm", false, "Reserva confirmada.");
    [RelayCommand] private Task CancelAsync(ReservationModel reservation) => TransitionAsync(reservation, "cancel", false, "Reserva cancelada.");
    [RelayCommand] private Task CompleteAsync(ReservationModel reservation) => TransitionAsync(reservation, "complete", PaymentReceivedOnSite, "Atendimento concluído.");
    [RelayCommand] private Task MarkNoShowAsync(ReservationModel reservation) => TransitionAsync(reservation, "no-show", false, "Ausência registrada.");

    private async Task TransitionAsync(ReservationModel reservation, string transition, bool paidOnSite, string success)
    {
        try { await api.TransitionPartnerReservationAsync(reservation.Id, transition, paidOnSite, CancellationToken.None); await LoadAsync(); StatusMessage = success; }
        catch (HttpRequestException) { StatusMessage = "A reserva não permite esta ação agora."; }
    }
}
