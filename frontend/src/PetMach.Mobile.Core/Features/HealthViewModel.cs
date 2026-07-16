using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PetMach.Mobile.Core.Features;

public sealed partial class HealthViewModel(IPetMachApiClient api, IDeviceFilePicker files) : ObservableObject
{
    public ObservableCollection<DogModel> Dogs { get; } = [];
    public ObservableCollection<VaccinationModel> Vaccinations { get; } = [];
    public ObservableCollection<DewormingModel> Dewormings { get; } = [];
    [ObservableProperty] private DogModel? selectedDog;
    [ObservableProperty] private string vaccineName = string.Empty;
    [ObservableProperty] private DateTime vaccineAppliedOn = DateTime.Today;
    [ObservableProperty] private DateTime vaccineNextDoseOn = DateTime.Today.AddYears(1);
    [ObservableProperty] private bool vaccineHasNextDose = true;
    [ObservableProperty] private string proofName = "Comprovante opcional";
    [ObservableProperty] private string dewormingName = string.Empty;
    [ObservableProperty] private DateTime dewormingAppliedOn = DateTime.Today;
    [ObservableProperty] private DateTime dewormingNextDoseOn = DateTime.Today.AddMonths(3);
    [ObservableProperty] private bool dewormingHasNextDose = true;
    [ObservableProperty] private bool vaccinationUpToDate;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = string.Empty;
    private PickedFile? proof;

    [RelayCommand]
    private async Task LoadDogsAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            Dogs.Clear();
            foreach (DogModel dog in await api.GetDogsAsync(CancellationToken.None)) Dogs.Add(dog);
            SelectedDog ??= Dogs.FirstOrDefault();
            if (SelectedDog is not null) await LoadHealthCoreAsync();
            else StatusMessage = "Cadastre um cão antes de registrar cuidados.";
        }
        catch (AuthenticationRequiredException ex) { StatusMessage = ex.Message; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível carregar a carteira de saúde."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task LoadHealthAsync()
    {
        if (SelectedDog is null || IsBusy) return;
        try { IsBusy = true; await LoadHealthCoreAsync(); }
        catch (HttpRequestException) { StatusMessage = "Não foi possível carregar a carteira de saúde."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task PickProofAsync()
    {
        proof = await files.PickHealthProofAsync(CancellationToken.None);
        ProofName = proof?.FileName ?? "Comprovante opcional";
    }

    [RelayCommand]
    private async Task AddVaccinationAsync()
    {
        if (SelectedDog is null || string.IsNullOrWhiteSpace(VaccineName) || IsBusy) return;
        try
        {
            IsBusy = true;
            VaccinationModel vaccination = await api.AddVaccinationAsync(SelectedDog.Id, VaccineName.Trim(), DateOnly.FromDateTime(VaccineAppliedOn), VaccineHasNextDose ? DateOnly.FromDateTime(VaccineNextDoseOn) : null, CancellationToken.None);
            if (proof is not null) await api.UploadVaccinationProofAsync(SelectedDog.Id, vaccination.Id, proof, CancellationToken.None);
            VaccineName = string.Empty;
            proof = null;
            ProofName = "Comprovante opcional";
            await LoadHealthCoreAsync();
            StatusMessage = "Vacina registrada com segurança.";
        }
        catch (AuthenticationRequiredException ex) { StatusMessage = ex.Message; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível registrar a vacina."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task AddDewormingAsync()
    {
        if (SelectedDog is null || string.IsNullOrWhiteSpace(DewormingName) || IsBusy) return;
        try
        {
            IsBusy = true;
            _ = await api.AddDewormingAsync(SelectedDog.Id, DewormingName.Trim(), DateOnly.FromDateTime(DewormingAppliedOn), DewormingHasNextDose ? DateOnly.FromDateTime(DewormingNextDoseOn) : null, CancellationToken.None);
            DewormingName = string.Empty;
            await LoadHealthCoreAsync();
            StatusMessage = "Vermífugo registrado.";
        }
        catch (AuthenticationRequiredException ex) { StatusMessage = ex.Message; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível registrar o vermífugo."; }
        finally { IsBusy = false; }
    }

    private async Task LoadHealthCoreAsync()
    {
        if (SelectedDog is null) return;
        DogHealthModel health = await api.GetDogHealthAsync(SelectedDog.Id, CancellationToken.None);
        Vaccinations.Clear();
        foreach (VaccinationModel item in health.Vaccinations) Vaccinations.Add(item);
        Dewormings.Clear();
        foreach (DewormingModel item in health.Dewormings) Dewormings.Add(item);
        VaccinationUpToDate = health.VaccinationUpToDate;
        StatusMessage = health.Disclaimer;
    }
}
