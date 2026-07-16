using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PetMach.Mobile.Core.Features;

public sealed partial class DiscoveryViewModel(IPetMachApiClient api) : ObservableObject
{
    public ObservableCollection<DogModel> MyDogs { get; } = [];
    private readonly Queue<DiscoveryDogModel> candidates = new();
    private int currentPage;
    private bool hasMore;
    [ObservableProperty] private DogModel? selectedDog;
    [ObservableProperty] private DiscoveryDogModel? currentDog;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private string breedFilter = string.Empty;
    [ObservableProperty] private DogSexModel? sexFilter;
    [ObservableProperty] private DogSizeModel? sizeFilter;
    [ObservableProperty] private EnergyLevelModel? energyLevelFilter;
    [ObservableProperty] private DogGoalModel? goalFilter;
    [ObservableProperty] private bool onlyNeutered;
    [ObservableProperty] private bool onlyVaccinated;

    public IReadOnlyList<DogSexModel> SexOptions { get; } = Enum.GetValues<DogSexModel>();
    public IReadOnlyList<DogSizeModel> SizeOptions { get; } = Enum.GetValues<DogSizeModel>();
    public IReadOnlyList<EnergyLevelModel> EnergyOptions { get; } = Enum.GetValues<EnergyLevelModel>();
    public IReadOnlyList<DogGoalModel> GoalOptions { get; } = Enum.GetValues<DogGoalModel>();

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            MyDogs.Clear();
            foreach (DogModel dog in await api.GetDogsAsync(CancellationToken.None)) MyDogs.Add(dog);
            SelectedDog ??= MyDogs.FirstOrDefault();
            await ReloadCandidatesAsync();
        }
        catch (AuthenticationRequiredException ex) { StatusMessage = ex.Message; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível carregar a descoberta."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (SelectedDog is null || IsBusy) return;
        try { IsBusy = true; await ReloadCandidatesAsync(); }
        catch (HttpRequestException) { StatusMessage = "Não foi possível atualizar a descoberta."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        BreedFilter = string.Empty;
        SexFilter = null;
        SizeFilter = null;
        EnergyLevelFilter = null;
        GoalFilter = null;
        OnlyNeutered = false;
        OnlyVaccinated = false;
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (SelectedDog is null || IsBusy || !hasMore) return;
        try
        {
            IsBusy = true;
            await LoadPageAsync(currentPage + 1);
            if (CurrentDog is null) ShowNext();
        }
        catch (HttpRequestException) { StatusMessage = "Não foi possível carregar mais perfis."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task LikeAsync()
    {
        if (SelectedDog is null || CurrentDog is null || IsBusy) return;
        try
        {
            IsBusy = true;
            LikeDogModel result = await api.LikeAsync(SelectedDog.Id, CurrentDog.DogId, CancellationToken.None);
            StatusMessage = result.MatchCreated ? "É um match! Vocês curtiram um ao outro." : "Curtida enviada.";
            ShowNext();
        }
        catch (HttpRequestException) { StatusMessage = "Não foi possível enviar a curtida."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task PassAsync()
    {
        if (SelectedDog is null || CurrentDog is null || IsBusy) return;
        try
        {
            IsBusy = true;
            await api.PassAsync(SelectedDog.Id, CurrentDog.DogId, CancellationToken.None);
            StatusMessage = "Perfil ignorado.";
            ShowNext();
        }
        catch (HttpRequestException) { StatusMessage = "Não foi possível ignorar o perfil."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task BlockAsync()
    {
        if (CurrentDog is null || IsBusy) return;
        try
        {
            IsBusy = true;
            await api.BlockDogOwnerAsync(CurrentDog.DogId, CancellationToken.None);
            StatusMessage = "Tutor bloqueado. Os perfis foram ocultados.";
            ShowNext();
        }
        catch (HttpRequestException) { StatusMessage = "Não foi possível bloquear este tutor."; }
        finally { IsBusy = false; }
    }

    private async Task ReloadCandidatesAsync()
    {
        candidates.Clear();
        CurrentDog = null;
        if (SelectedDog is null)
        {
            StatusMessage = "Cadastre um cão para começar a descoberta.";
            return;
        }
        await LoadPageAsync(1);
        ShowNext();
    }

    private async Task LoadPageAsync(int pageNumber)
    {
        DiscoveryFilterModel filter = new(
            SexFilter, SizeFilter, Clean(BreedFilter), EnergyLevelFilter, GoalFilter,
            OnlyNeutered ? true : null, OnlyVaccinated ? true : null, pageNumber);
        DiscoveryPageModel page = await api.DiscoverAsync(SelectedDog!.Id, filter, CancellationToken.None);
        foreach (DiscoveryDogModel dog in page.Items) candidates.Enqueue(dog);
        currentPage = page.Page;
        hasMore = page.HasMore;
    }

    private void ShowNext()
    {
        CurrentDog = candidates.TryDequeue(out DiscoveryDogModel? dog) ? dog : null;
        if (CurrentDog is null) StatusMessage = hasMore
            ? "Há mais perfis disponíveis. Carregue a próxima página."
            : "Não há mais perfis com esses critérios por enquanto.";
    }

    private static string? Clean(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
