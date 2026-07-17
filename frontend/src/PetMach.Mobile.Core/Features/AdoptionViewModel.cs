using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PetMach.Mobile.Core.Features;

public sealed partial class AdoptionViewModel(IPetMachApiClient api, IDeviceFilePicker filePicker) : ObservableObject
{
    public ObservableCollection<DogModel> Dogs { get; } = [];
    public ObservableCollection<AdoptionProfileModel> Profiles { get; } = [];
    public ObservableCollection<AdoptionApplicationModel> Applications { get; } = [];
    public IReadOnlyList<string> ReportReasons { get; } = ["AnimalWelfare", "Fraud", "UnsafeContent", "Spam", "Harassment", "Other"];
    [ObservableProperty] private DogModel? selectedDog;
    [ObservableProperty] private AdoptionProfileModel? selectedProfile;
    [ObservableProperty] private string story = string.Empty;
    [ObservableProperty] private string requirements = string.Empty;
    [ObservableProperty] private bool publicationTermsAccepted;
    [ObservableProperty] private string motivation = string.Empty;
    [ObservableProperty] private string experience = string.Empty;
    [ObservableProperty] private string housingContext = string.Empty;
    [ObservableProperty] private bool applicationTermsAccepted;
    [ObservableProperty] private string selectedReportReason = "AnimalWelfare";
    [ObservableProperty] private string reportDescription = string.Empty;
    [ObservableProperty] private PickedFile? reportEvidence;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = string.Empty;

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true; StatusMessage = string.Empty; Dogs.Clear(); Profiles.Clear(); Applications.Clear();
            foreach (DogModel dog in await api.GetDogsAsync(CancellationToken.None)) Dogs.Add(dog);
            foreach (AdoptionProfileModel profile in await api.GetAdoptionProfilesAsync(CancellationToken.None)) Profiles.Add(profile);
            foreach (AdoptionApplicationModel application in await api.GetMyAdoptionApplicationsAsync(CancellationToken.None)) Applications.Add(application);
        }
        catch (AuthenticationRequiredException ex) { StatusMessage = ex.Message; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível carregar a área de adoção."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task PublishAsync()
    {
        if (SelectedDog is null || !PublicationTermsAccepted || string.IsNullOrWhiteSpace(Story) || string.IsNullOrWhiteSpace(Requirements))
        { StatusMessage = "Selecione o cão, preencha os textos e aceite o termo."; return; }
        try { await api.CreateAdoptionProfileAsync(SelectedDog.Id, Story, Requirements, CancellationToken.None); await LoadAsync(); StatusMessage = "Publicação criada."; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível criar a publicação."; }
    }

    [RelayCommand]
    private async Task ApplyAsync()
    {
        if (SelectedProfile is null || !SelectedProfile.CanApply || !ApplicationTermsAccepted || string.IsNullOrWhiteSpace(Motivation) || string.IsNullOrWhiteSpace(Experience) || string.IsNullOrWhiteSpace(HousingContext))
        { StatusMessage = "Selecione uma publicação, preencha a candidatura e aceite o termo."; return; }
        try { await api.ApplyForAdoptionAsync(SelectedProfile.Id, Motivation, Experience, HousingContext, CancellationToken.None); await LoadAsync(); StatusMessage = "Candidatura enviada."; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível enviar a candidatura."; }
    }

    [RelayCommand]
    private async Task SuspendAsync(AdoptionProfileModel? profile)
    {
        if (profile is null || !profile.CanSuspend) return;
        try { await api.SuspendAdoptionProfileAsync(profile.Id, CancellationToken.None); await LoadAsync(); StatusMessage = "Publicação suspensa."; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível suspender a publicação."; }
    }

    [RelayCommand]
    private async Task WithdrawAsync(AdoptionApplicationModel? application)
    {
        if (application is null || !application.CanWithdraw) return;
        try { await api.TransitionAdoptionApplicationAsync(application.Id, "withdraw", CancellationToken.None); await LoadAsync(); StatusMessage = "Candidatura retirada."; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível retirar a candidatura."; }
    }

    [RelayCommand]
    private async Task PickEvidenceAsync()
    {
        ReportEvidence = await filePicker.PickReportEvidenceAsync(CancellationToken.None);
        StatusMessage = ReportEvidence is null ? string.Empty : $"Evidência selecionada: {ReportEvidence.FileName}";
    }

    [RelayCommand]
    private async Task ReportAsync()
    {
        if (SelectedProfile is null || SelectedProfile.IsMine || string.IsNullOrWhiteSpace(ReportDescription)) { StatusMessage = "Selecione uma publicação de outro tutor e descreva o problema."; return; }
        try
        {
            ReportModel report = await api.ReportAdoptionProfileAsync(SelectedProfile.Id, SelectedReportReason, ReportDescription, CancellationToken.None);
            if (ReportEvidence is not null) await api.UploadReportEvidenceAsync(report.Id, ReportEvidence, CancellationToken.None);
            ReportEvidence = null; StatusMessage = "Denúncia enviada para moderação.";
        }
        catch (HttpRequestException) { StatusMessage = "Não foi possível enviar a denúncia."; }
    }
}
