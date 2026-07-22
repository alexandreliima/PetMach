using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PetMach.Mobile.Core.Features;

public enum DiscoveryScreenState
{
    Initial,
    Loading,
    Content,
    Empty,
    Error,
}

public enum DiscoveryFeedbackKind
{
    None,
    Success,
    Error,
}

public sealed partial class DiscoveryViewModel(IPetMachApiClient api) : ObservableObject
{
    private readonly object lifecycleSync = new();
    private CancellationTokenSource? activationCancellation;
    private CancellationTokenSource? contextCancellation;
    private Task? activationTask;
    private long contextVersion;
    private int actionInProgress;
    private int paginationInProgress;
    private int dogsLoaded;
    private int currentPage;
    private bool hasMore;
    private bool isActive;
    private DogModel? selectedDog;

    public ObservableCollection<DogModel> MyDogs { get; } = [];
    public ObservableCollection<DiscoveryDogModel> Candidates { get; } = [];

    [ObservableProperty]
    private DiscoveryScreenState screenState = DiscoveryScreenState.Initial;

    [ObservableProperty]
    private DiscoveryFeedbackKind feedbackKind;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private string emptyMessage = "Não encontramos novos pets no momento.";

    [ObservableProperty]
    private bool isPaging;

    [ObservableProperty]
    private bool isActionInProgress;

    [ObservableProperty]
    private string breedFilter = string.Empty;

    [ObservableProperty]
    private DogSexModel? sexFilter;

    [ObservableProperty]
    private DogSizeModel? sizeFilter;

    [ObservableProperty]
    private EnergyLevelModel? energyLevelFilter;

    [ObservableProperty]
    private DogGoalModel? goalFilter;

    [ObservableProperty]
    private bool onlyNeutered;

    [ObservableProperty]
    private bool onlyVaccinated;

    [ObservableProperty]
    private bool showFilters;

    public DogModel? SelectedDog
    {
        get => selectedDog;
        private set => SetProperty(ref selectedDog, value);
    }

    public bool IsActive
    {
        get
        {
            lock (lifecycleSync)
            {
                return isActive;
            }
        }
    }

    public bool IsInitialState => ScreenState == DiscoveryScreenState.Initial;
    public bool IsLoadingState => ScreenState == DiscoveryScreenState.Loading;
    public bool IsContentState => ScreenState == DiscoveryScreenState.Content;
    public bool IsEmptyState => ScreenState == DiscoveryScreenState.Empty;
    public bool IsErrorState => ScreenState == DiscoveryScreenState.Error;
    public bool HasSuccessFeedback => FeedbackKind == DiscoveryFeedbackKind.Success;
    public bool HasErrorFeedback => FeedbackKind == DiscoveryFeedbackKind.Error;
    public bool CanPerformActions =>
        IsActive &&
        IsContentState &&
        !IsPaging &&
        !IsActionInProgress;
    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(BreedFilter) ||
        SexFilter.HasValue ||
        SizeFilter.HasValue ||
        EnergyLevelFilter.HasValue ||
        GoalFilter.HasValue ||
        OnlyNeutered ||
        OnlyVaccinated;

    public IReadOnlyList<DogSexModel> SexOptions { get; } = Enum.GetValues<DogSexModel>();
    public IReadOnlyList<DogSizeModel> SizeOptions { get; } = Enum.GetValues<DogSizeModel>();
    public IReadOnlyList<EnergyLevelModel> EnergyOptions { get; } = Enum.GetValues<EnergyLevelModel>();
    public IReadOnlyList<DogGoalModel> GoalOptions { get; } = Enum.GetValues<DogGoalModel>();

    public Task ActivateAsync(CancellationToken cancellationToken)
    {
        lock (lifecycleSync)
        {
            if (isActive)
            {
                return activationTask ?? Task.CompletedTask;
            }

            isActive = true;
            activationCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);
            activationTask = LoadInitialCoreAsync(activationCancellation.Token);
            return activationTask;
        }
    }

    public void Deactivate()
    {
        CancellationTokenSource? previousContext;
        CancellationTokenSource? previousActivation;
        lock (lifecycleSync)
        {
            isActive = false;
            activationTask = null;
            contextVersion++;
            previousContext = contextCancellation;
            previousActivation = activationCancellation;
            contextCancellation = null;
            activationCancellation = null;
        }

        previousContext?.Cancel();
        previousContext?.Dispose();
        previousActivation?.Cancel();
        previousActivation?.Dispose();
        IsPaging = false;
        IsActionInProgress = false;
        ClearFeedback();
        ErrorMessage = string.Empty;
        ScreenState = DiscoveryScreenState.Initial;
        NotifyCommandStates();
    }

    public async Task SelectDogAsync(DogModel? dog, CancellationToken cancellationToken)
    {
        if (!IsActive || SelectedDog?.Id == dog?.Id)
        {
            return;
        }

        SelectedDog = dog;
        await ReloadCandidatesAsync(cancellationToken);
    }

    [RelayCommand]
    private Task LoadAsync(CancellationToken cancellationToken) =>
        ActivateAsync(cancellationToken);

    [RelayCommand]
    private void ToggleFilters() => ShowFilters = !ShowFilters;

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private Task RefreshAsync(CancellationToken cancellationToken) =>
        ReloadCandidatesAsync(cancellationToken);

    [RelayCommand(CanExecute = nameof(CanClearFilters))]
    private async Task ClearFiltersAsync(CancellationToken cancellationToken)
    {
        BreedFilter = string.Empty;
        SexFilter = null;
        SizeFilter = null;
        EnergyLevelFilter = null;
        GoalFilter = null;
        OnlyNeutered = false;
        OnlyVaccinated = false;
        await ReloadCandidatesAsync(cancellationToken);
    }

    [RelayCommand(CanExecute = nameof(CanRetry))]
    private Task RetryAsync(CancellationToken cancellationToken) =>
        Volatile.Read(ref dogsLoaded) == 0
            ? LoadInitialCoreAsync(cancellationToken)
            : ReloadCandidatesAsync(cancellationToken);

    [RelayCommand(
        CanExecute = nameof(CanLoadMore),
        AllowConcurrentExecutions = true)]
    private async Task LoadMoreAsync(CancellationToken _)
    {
        if (Interlocked.CompareExchange(ref paginationInProgress, 1, 0) != 0)
        {
            return;
        }

        ContextSnapshot? context = GetCurrentContext();
        if (context is null || !CanLoadMore())
        {
            Interlocked.Exchange(ref paginationInProgress, 0);
            return;
        }

        IsPaging = true;
        NotifyCommandStates();
        try
        {
            DiscoveryPageModel page = await FetchPageAsync(
                currentPage + 1,
                context.Value.Token);
            context.Value.Token.ThrowIfCancellationRequested();
            if (!IsCurrent(context.Value))
            {
                return;
            }

            foreach (DiscoveryDogModel dog in page.Items)
            {
                if (!Candidates.Any(existing => existing.DogId == dog.DogId))
                {
                    Candidates.Add(dog);
                }
            }

            currentPage = page.Page;
            hasMore = page.HasMore;
            ClearFeedback();
        }
        catch (OperationCanceledException) when (
            context.Value.Token.IsCancellationRequested)
        {
        }
        catch (AuthenticationRequiredException exception)
        {
            if (IsCurrent(context.Value))
            {
                SetFeedback(DiscoveryFeedbackKind.Error, exception.Message);
            }
        }
        catch (HttpRequestException)
        {
            if (IsCurrent(context.Value))
            {
                SetFeedback(
                    DiscoveryFeedbackKind.Error,
                    "Não foi possível carregar mais perfis.");
            }
        }
        finally
        {
            Interlocked.Exchange(ref paginationInProgress, 0);
            if (IsCurrent(context.Value))
            {
                IsPaging = false;
                NotifyCommandStates();
            }
        }
    }

    [RelayCommand(
        CanExecute = nameof(CanInteractWithCandidate),
        AllowConcurrentExecutions = true)]
    private Task LikeAsync(
        DiscoveryDogModel? dog,
        CancellationToken _) =>
        ExecuteCandidateActionAsync(
            dog,
            async (sourceDog, candidate, token) =>
            {
                LikeDogModel result = await api.LikeAsync(
                    sourceDog.Id,
                    candidate.DogId,
                    token);
                return result.MatchCreated
                    ? "É um match! Agora vocês podem conversar e combinar um encontro."
                    : "Curtida enviada.";
            });

    [RelayCommand(
        CanExecute = nameof(CanInteractWithCandidate),
        AllowConcurrentExecutions = true)]
    private Task PassAsync(
        DiscoveryDogModel? dog,
        CancellationToken _) =>
        ExecuteCandidateActionAsync(
            dog,
            async (sourceDog, candidate, token) =>
            {
                await api.PassAsync(sourceDog.Id, candidate.DogId, token);
                return "Perfil ignorado.";
            });

    [RelayCommand(
        CanExecute = nameof(CanInteractWithCandidate),
        AllowConcurrentExecutions = true)]
    private Task BlockAsync(
        DiscoveryDogModel? dog,
        CancellationToken _) =>
        ExecuteCandidateActionAsync(
            dog,
            async (_, candidate, token) =>
            {
                await api.BlockDogOwnerAsync(candidate.DogId, token);
                return "Tutor bloqueado. Os perfis foram ocultados.";
            });

    private async Task LoadInitialCoreAsync(CancellationToken cancellationToken)
    {
        Interlocked.Exchange(ref dogsLoaded, 0);
        ContextSnapshot context = ReplaceContext();
        PrepareForReload();
        try
        {
            using CancellationTokenSource operation = CancellationTokenSource.CreateLinkedTokenSource(
                context.Token,
                cancellationToken);
            IReadOnlyCollection<DogModel> dogs = await api.GetDogsAsync(operation.Token);
            operation.Token.ThrowIfCancellationRequested();
            if (!IsCurrent(context))
            {
                return;
            }

            Guid? previousDogId = SelectedDog?.Id;
            MyDogs.Clear();
            foreach (DogModel dog in dogs)
            {
                MyDogs.Add(dog);
            }

            Interlocked.Exchange(ref dogsLoaded, 1);
            SelectedDog = previousDogId.HasValue
                ? MyDogs.FirstOrDefault(dog => dog.Id == previousDogId.Value)
                    ?? MyDogs.FirstOrDefault()
                : MyDogs.FirstOrDefault();

            if (SelectedDog is null)
            {
                EmptyMessage = "Cadastre um cão para começar a explorar a rede.";
                ScreenState = DiscoveryScreenState.Empty;
                return;
            }

            DiscoveryPageModel page = await FetchPageAsync(1, operation.Token);
            operation.Token.ThrowIfCancellationRequested();
            if (IsCurrent(context))
            {
                ApplyFirstPage(page);
            }
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested ||
            context.Token.IsCancellationRequested)
        {
        }
        catch (AuthenticationRequiredException exception)
        {
            if (IsCurrent(context))
            {
                ShowError(exception.Message);
            }
        }
        catch (HttpRequestException)
        {
            if (IsCurrent(context))
            {
                ShowError("Não foi possível carregar a rede.");
            }
        }
    }

    private async Task ReloadCandidatesAsync(CancellationToken cancellationToken)
    {
        if (!IsActive)
        {
            return;
        }

        ContextSnapshot context = ReplaceContext();
        PrepareForReload();
        if (SelectedDog is null)
        {
            EmptyMessage = "Cadastre um cão para começar a explorar a rede.";
            ScreenState = DiscoveryScreenState.Empty;
            return;
        }

        try
        {
            using CancellationTokenSource operation = CancellationTokenSource.CreateLinkedTokenSource(
                context.Token,
                cancellationToken);
            DiscoveryPageModel page = await FetchPageAsync(1, operation.Token);
            operation.Token.ThrowIfCancellationRequested();
            if (IsCurrent(context))
            {
                ApplyFirstPage(page);
            }
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested ||
            context.Token.IsCancellationRequested)
        {
        }
        catch (AuthenticationRequiredException exception)
        {
            if (IsCurrent(context))
            {
                ShowError(exception.Message);
            }
        }
        catch (HttpRequestException)
        {
            if (IsCurrent(context))
            {
                ShowError("Não foi possível atualizar a rede.");
            }
        }
    }

    private async Task ExecuteCandidateActionAsync(
        DiscoveryDogModel? dog,
        Func<DogModel, DiscoveryDogModel, CancellationToken, Task<string>> action)
    {
        if (!CanInteractWithCandidate(dog) ||
            Interlocked.CompareExchange(ref actionInProgress, 1, 0) != 0)
        {
            return;
        }

        ContextSnapshot? context = GetCurrentContext();
        DogModel? sourceDog = SelectedDog;
        if (context is null || sourceDog is null || dog is null)
        {
            Interlocked.Exchange(ref actionInProgress, 0);
            return;
        }

        IsActionInProgress = true;
        ClearFeedback();
        NotifyCommandStates();
        try
        {
            string successMessage = await action(
                sourceDog,
                dog,
                context.Value.Token);
            context.Value.Token.ThrowIfCancellationRequested();
            if (!IsCurrent(context.Value))
            {
                return;
            }

            Candidates.Remove(dog);
            SetFeedback(DiscoveryFeedbackKind.Success, successMessage);
            if (Candidates.Count == 0)
            {
                EmptyMessage = "Não encontramos novos pets no momento.";
                ScreenState = DiscoveryScreenState.Empty;
            }
        }
        catch (OperationCanceledException) when (
            context.Value.Token.IsCancellationRequested)
        {
        }
        catch (AuthenticationRequiredException exception)
        {
            if (IsCurrent(context.Value))
            {
                SetFeedback(DiscoveryFeedbackKind.Error, exception.Message);
            }
        }
        catch (HttpRequestException)
        {
            if (IsCurrent(context.Value))
            {
                SetFeedback(
                    DiscoveryFeedbackKind.Error,
                    "Não foi possível concluir esta ação. O perfil foi preservado.");
            }
        }
        finally
        {
            Interlocked.Exchange(ref actionInProgress, 0);
            if (IsActive)
            {
                IsActionInProgress = false;
                NotifyCommandStates();
            }
        }
    }

    private Task<DiscoveryPageModel> FetchPageAsync(
        int pageNumber,
        CancellationToken cancellationToken)
    {
        DiscoveryFilterModel filter = new(
            SexFilter,
            SizeFilter,
            Clean(BreedFilter),
            EnergyLevelFilter,
            GoalFilter,
            OnlyNeutered ? true : null,
            OnlyVaccinated ? true : null,
            pageNumber);
        return api.DiscoverAsync(SelectedDog!.Id, filter, cancellationToken);
    }

    private void PrepareForReload()
    {
        Candidates.Clear();
        currentPage = 0;
        hasMore = false;
        IsPaging = false;
        ErrorMessage = string.Empty;
        EmptyMessage = "Não encontramos novos pets no momento.";
        ClearFeedback();
        ScreenState = DiscoveryScreenState.Loading;
        NotifyCommandStates();
    }

    private void ApplyFirstPage(DiscoveryPageModel page)
    {
        Candidates.Clear();
        foreach (DiscoveryDogModel dog in page.Items)
        {
            Candidates.Add(dog);
        }

        currentPage = page.Page;
        hasMore = page.HasMore;
        ErrorMessage = string.Empty;
        ClearFeedback();
        ScreenState = Candidates.Count == 0
            ? DiscoveryScreenState.Empty
            : DiscoveryScreenState.Content;
        NotifyCommandStates();
    }

    private void ShowError(string message)
    {
        ErrorMessage = message;
        ScreenState = DiscoveryScreenState.Error;
        NotifyCommandStates();
    }

    private void SetFeedback(DiscoveryFeedbackKind kind, string message)
    {
        StatusMessage = message;
        FeedbackKind = kind;
    }

    private void ClearFeedback()
    {
        StatusMessage = string.Empty;
        FeedbackKind = DiscoveryFeedbackKind.None;
    }

    private ContextSnapshot ReplaceContext()
    {
        CancellationTokenSource? previous;
        ContextSnapshot result;
        lock (lifecycleSync)
        {
            if (!isActive || activationCancellation is null)
            {
                throw new InvalidOperationException(
                    "A descoberta precisa estar ativa para criar um contexto.");
            }

            previous = contextCancellation;
            contextCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                activationCancellation.Token);
            contextVersion++;
            result = new ContextSnapshot(contextVersion, contextCancellation.Token);
        }

        previous?.Cancel();
        previous?.Dispose();
        return result;
    }

    private ContextSnapshot? GetCurrentContext()
    {
        lock (lifecycleSync)
        {
            return !isActive || contextCancellation is null
                ? null
                : new ContextSnapshot(contextVersion, contextCancellation.Token);
        }
    }

    private bool IsCurrent(ContextSnapshot context)
    {
        lock (lifecycleSync)
        {
            return isActive &&
                contextVersion == context.Version &&
                !context.Token.IsCancellationRequested;
        }
    }

    private bool CanRefresh() =>
        IsActive &&
        SelectedDog is not null &&
        !IsLoadingState &&
        !IsActionInProgress;

    private bool CanClearFilters() => CanRefresh() && HasActiveFilters;

    private bool CanRetry() => IsActive && IsErrorState && !IsActionInProgress;

    private bool CanLoadMore() =>
        IsActive &&
        IsContentState &&
        SelectedDog is not null &&
        hasMore &&
        !IsPaging &&
        !IsActionInProgress;

    private bool CanInteractWithCandidate(DiscoveryDogModel? dog) =>
        CanPerformActions &&
        dog is not null &&
        Candidates.Contains(dog);

    private void NotifyCommandStates()
    {
        RefreshCommand.NotifyCanExecuteChanged();
        ClearFiltersCommand.NotifyCanExecuteChanged();
        RetryCommand.NotifyCanExecuteChanged();
        LoadMoreCommand.NotifyCanExecuteChanged();
        LikeCommand.NotifyCanExecuteChanged();
        PassCommand.NotifyCanExecuteChanged();
        BlockCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanPerformActions));
    }

    partial void OnScreenStateChanged(DiscoveryScreenState value)
    {
        OnPropertyChanged(nameof(IsInitialState));
        OnPropertyChanged(nameof(IsLoadingState));
        OnPropertyChanged(nameof(IsContentState));
        OnPropertyChanged(nameof(IsEmptyState));
        OnPropertyChanged(nameof(IsErrorState));
        NotifyCommandStates();
    }

    partial void OnFeedbackKindChanged(DiscoveryFeedbackKind value)
    {
        OnPropertyChanged(nameof(HasSuccessFeedback));
        OnPropertyChanged(nameof(HasErrorFeedback));
    }

    partial void OnIsPagingChanged(bool value) => NotifyCommandStates();

    partial void OnIsActionInProgressChanged(bool value) => NotifyCommandStates();

    partial void OnBreedFilterChanged(string value) => NotifyFilterStateChanged();
    partial void OnSexFilterChanged(DogSexModel? value) => NotifyFilterStateChanged();
    partial void OnSizeFilterChanged(DogSizeModel? value) => NotifyFilterStateChanged();
    partial void OnEnergyLevelFilterChanged(EnergyLevelModel? value) => NotifyFilterStateChanged();
    partial void OnGoalFilterChanged(DogGoalModel? value) => NotifyFilterStateChanged();
    partial void OnOnlyNeuteredChanged(bool value) => NotifyFilterStateChanged();
    partial void OnOnlyVaccinatedChanged(bool value) => NotifyFilterStateChanged();

    private void NotifyFilterStateChanged()
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        ClearFiltersCommand.NotifyCanExecuteChanged();
    }

    private static string? Clean(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private readonly record struct ContextSnapshot(
        long Version,
        CancellationToken Token);
}
