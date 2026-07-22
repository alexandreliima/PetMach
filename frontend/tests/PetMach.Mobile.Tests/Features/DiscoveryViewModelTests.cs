using FluentAssertions;
using NSubstitute;
using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile.Tests.Features;

public sealed class DiscoveryViewModelTests
{
    [Fact]
    public async Task InitialLoadShouldExposeContentAndTheSelectedSourceDog()
    {
        DogModel source = SourceDog("Mel");
        DiscoveryDogModel candidate = Candidate("Lua");
        IPetMachApiClient api = CreateApi([source], Page([candidate]));
        DiscoveryViewModel viewModel = new(api);

        await viewModel.ActivateAsync(TestCancellation.Token);

        viewModel.ScreenState.Should().Be(DiscoveryScreenState.Content);
        viewModel.SelectedDog.Should().Be(source);
        viewModel.Candidates.Should().ContainSingle().Which.Should().Be(candidate);
        viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task InitialLoadWithoutCandidatesShouldExposeEmptyState()
    {
        IPetMachApiClient api = CreateApi([SourceDog("Mel")], Page([]));
        DiscoveryViewModel viewModel = new(api);

        await viewModel.ActivateAsync(TestCancellation.Token);

        viewModel.ScreenState.Should().Be(DiscoveryScreenState.Empty);
        viewModel.EmptyMessage.Should().Be("Não encontramos novos pets no momento.");
    }

    [Fact]
    public async Task InitialFailureShouldAllowASuccessfulRetry()
    {
        DogModel source = SourceDog("Mel");
        DiscoveryDogModel candidate = Candidate("Lua");
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        api.GetDogsAsync(Arg.Any<CancellationToken>()).Returns([source]);
        int attempts = 0;
        api.DiscoverAsync(source.Id, Arg.Any<DiscoveryFilterModel>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                attempts++;
                return attempts == 1
                    ? Task.FromException<DiscoveryPageModel>(new HttpRequestException())
                    : Task.FromResult(Page([candidate]));
            });
        DiscoveryViewModel viewModel = new(api);

        await viewModel.ActivateAsync(TestCancellation.Token);
        viewModel.ScreenState.Should().Be(DiscoveryScreenState.Error);

        await viewModel.RetryCommand.ExecuteAsync(null);

        viewModel.ScreenState.Should().Be(DiscoveryScreenState.Content);
        viewModel.Candidates.Should().ContainSingle().Which.Should().Be(candidate);
        viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task ConcurrentActivationShouldShareTheInitialLoad()
    {
        DogModel source = SourceDog("Mel");
        TaskCompletionSource<IReadOnlyCollection<DogModel>> dogsGate =
            NewCompletionSource<IReadOnlyCollection<DogModel>>();
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        api.GetDogsAsync(Arg.Any<CancellationToken>())
            .Returns(call => dogsGate.Task.WaitAsync(call.Arg<CancellationToken>()));
        api.DiscoverAsync(source.Id, Arg.Any<DiscoveryFilterModel>(), Arg.Any<CancellationToken>())
            .Returns(Page([]));
        DiscoveryViewModel viewModel = new(api);

        Task first = viewModel.ActivateAsync(TestCancellation.Token);
        Task second = viewModel.ActivateAsync(TestCancellation.Token);
        dogsGate.SetResult([source]);
        await Task.WhenAll(first, second);

        await api.Received(1).GetDogsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivationShouldCancelAndIgnoreALateResponse()
    {
        TaskCompletionSource<IReadOnlyCollection<DogModel>> dogsGate =
            NewCompletionSource<IReadOnlyCollection<DogModel>>();
        CancellationToken requestToken = default;
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        api.GetDogsAsync(Arg.Any<CancellationToken>()).Returns(call =>
        {
            requestToken = call.Arg<CancellationToken>();
            return dogsGate.Task;
        });
        DiscoveryViewModel viewModel = new(api);

        Task activation = viewModel.ActivateAsync(TestCancellation.Token);
        viewModel.Deactivate();
        dogsGate.SetResult([SourceDog("Mel")]);
        await activation;

        viewModel.IsActive.Should().BeFalse();
        requestToken.IsCancellationRequested.Should().BeTrue();
        viewModel.MyDogs.Should().BeEmpty();
        viewModel.Candidates.Should().BeEmpty();
        viewModel.ScreenState.Should().Be(DiscoveryScreenState.Initial);
        viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task ANewActivationAfterDisappearingShouldLoadAgain()
    {
        DogModel source = SourceDog("Mel");
        IPetMachApiClient api = CreateApi([source], Page([]));
        DiscoveryViewModel viewModel = new(api);

        await viewModel.ActivateAsync(TestCancellation.Token);
        viewModel.Deactivate();
        await viewModel.ActivateAsync(TestCancellation.Token);

        await api.Received(2).GetDogsAsync(Arg.Any<CancellationToken>());
        viewModel.IsActive.Should().BeTrue();
        viewModel.ScreenState.Should().Be(DiscoveryScreenState.Empty);
    }

    [Fact]
    public async Task SelectingAnotherDogShouldReplaceThePreviousCandidates()
    {
        DogModel firstSource = SourceDog("Mel");
        DogModel secondSource = SourceDog("Thor");
        DiscoveryDogModel firstCandidate = Candidate("Lua");
        DiscoveryDogModel secondCandidate = Candidate("Nina");
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        api.GetDogsAsync(Arg.Any<CancellationToken>()).Returns([firstSource, secondSource]);
        api.DiscoverAsync(
                Arg.Any<Guid>(),
                Arg.Any<DiscoveryFilterModel>(),
                Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Guid>() == firstSource.Id
                ? Page([firstCandidate])
                : Page([secondCandidate]));
        DiscoveryViewModel viewModel = new(api);
        await viewModel.ActivateAsync(TestCancellation.Token);

        await viewModel.SelectDogAsync(secondSource, TestCancellation.Token);

        viewModel.SelectedDog.Should().Be(secondSource);
        viewModel.Candidates.Should().ContainSingle().Which.Should().Be(secondCandidate);
        viewModel.StatusMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task AResponseFromThePreviousDogShouldNotMixWithTheCurrentDog()
    {
        DogModel initialSource = SourceDog("Mel");
        DogModel slowSource = SourceDog("Thor");
        DogModel currentSource = SourceDog("Lola");
        DiscoveryDogModel currentCandidate = Candidate("Nina");
        TaskCompletionSource<DiscoveryPageModel> slowGate =
            NewCompletionSource<DiscoveryPageModel>();
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        api.GetDogsAsync(Arg.Any<CancellationToken>())
            .Returns([initialSource, slowSource, currentSource]);
        api.DiscoverAsync(
                Arg.Any<Guid>(),
                Arg.Any<DiscoveryFilterModel>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                Guid sourceId = call.Arg<Guid>();
                if (sourceId == slowSource.Id)
                {
                    return slowGate.Task;
                }

                return Task.FromResult(sourceId == currentSource.Id
                    ? Page([currentCandidate])
                    : Page([]));
            });
        DiscoveryViewModel viewModel = new(api);
        await viewModel.ActivateAsync(TestCancellation.Token);

        Task slowSelection = viewModel.SelectDogAsync(slowSource, TestCancellation.Token);
        await viewModel.SelectDogAsync(currentSource, TestCancellation.Token);
        slowGate.SetResult(Page([Candidate("Resposta antiga")]));
        await slowSelection;

        viewModel.SelectedDog.Should().Be(currentSource);
        viewModel.Candidates.Should().ContainSingle().Which.Should().Be(currentCandidate);
    }

    [Fact]
    public async Task ApplyingFiltersShouldRestartAtTheFirstPage()
    {
        DogModel source = SourceDog("Mel");
        DiscoveryFilterModel? capturedFilter = null;
        IPetMachApiClient api = CreateApi([source], Page([Candidate("Lua")]));
        api.DiscoverAsync(source.Id, Arg.Any<DiscoveryFilterModel>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                capturedFilter = call.Arg<DiscoveryFilterModel>();
                return Page([Candidate("Lua")]);
            });
        DiscoveryViewModel viewModel = new(api);
        await viewModel.ActivateAsync(TestCancellation.Token);
        viewModel.BreedFilter = "  SRD  ";
        viewModel.SizeFilter = DogSizeModel.Medium;
        viewModel.OnlyVaccinated = true;

        await viewModel.RefreshCommand.ExecuteAsync(null);

        capturedFilter.Should().NotBeNull();
        capturedFilter!.Page.Should().Be(1);
        capturedFilter.Breed.Should().Be("SRD");
        capturedFilter.Size.Should().Be(DogSizeModel.Medium);
        capturedFilter.VaccinationUpToDate.Should().BeTrue();
    }

    [Fact]
    public async Task ClearingFiltersShouldResetAllSevenFiltersAndReload()
    {
        DogModel source = SourceDog("Mel");
        IPetMachApiClient api = CreateApi([source], Page([Candidate("Lua")]));
        DiscoveryViewModel viewModel = new(api);
        await viewModel.ActivateAsync(TestCancellation.Token);
        viewModel.BreedFilter = "SRD";
        viewModel.SexFilter = DogSexModel.Female;
        viewModel.SizeFilter = DogSizeModel.Medium;
        viewModel.EnergyLevelFilter = EnergyLevelModel.Moderate;
        viewModel.GoalFilter = DogGoalModel.Friendship;
        viewModel.OnlyNeutered = true;
        viewModel.OnlyVaccinated = true;

        await viewModel.ClearFiltersCommand.ExecuteAsync(null);

        viewModel.HasActiveFilters.Should().BeFalse();
        viewModel.BreedFilter.Should().BeEmpty();
        viewModel.SexFilter.Should().BeNull();
        viewModel.SizeFilter.Should().BeNull();
        viewModel.EnergyLevelFilter.Should().BeNull();
        viewModel.GoalFilter.Should().BeNull();
        viewModel.OnlyNeutered.Should().BeFalse();
        viewModel.OnlyVaccinated.Should().BeFalse();
        await api.Received(2).DiscoverAsync(
            source.Id,
            Arg.Any<DiscoveryFilterModel>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PaginationShouldAllowOnlyOneRequestAtATime()
    {
        DogModel source = SourceDog("Mel");
        TaskCompletionSource<DiscoveryPageModel> pageGate =
            NewCompletionSource<DiscoveryPageModel>();
        TaskCompletionSource pageStarted =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        int pageTwoCalls = 0;
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        api.GetDogsAsync(Arg.Any<CancellationToken>()).Returns([source]);
        api.DiscoverAsync(source.Id, Arg.Any<DiscoveryFilterModel>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                if (call.ArgAt<DiscoveryFilterModel>(1)!.Page == 1)
                {
                    return Task.FromResult(Page([Candidate("Lua")], hasMore: true));
                }

                Interlocked.Increment(ref pageTwoCalls);
                pageStarted.TrySetResult();
                return pageGate.Task.WaitAsync(call.Arg<CancellationToken>());
            });
        DiscoveryViewModel viewModel = new(api);
        await viewModel.ActivateAsync(TestCancellation.Token);

        Task first = viewModel.LoadMoreCommand.ExecuteAsync(null);
        await pageStarted.Task.WaitAsync(TestCancellation.Token);
        first.IsCompleted.Should().BeFalse();
        viewModel.IsPaging.Should().BeTrue();
        Task second = viewModel.LoadMoreCommand.ExecuteAsync(null);
        pageGate.SetResult(Page([Candidate("Thor")], page: 2));
        await Task.WhenAll(first, second);

        int recordedPageTwoCalls = api.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(IPetMachApiClient.DiscoverAsync))
            .Select(call => call.GetArguments()[1] as DiscoveryFilterModel)
            .Count(filter => filter?.Page == 2);
        recordedPageTwoCalls.Should().Be(1);
        pageTwoCalls.Should().Be(1);
        viewModel.Candidates.Should().HaveCount(2);
    }

    [Fact]
    public async Task CandidateActionsShouldRemainDisabledWhilePaginationIsPending()
    {
        DogModel source = SourceDog("Mel");
        DiscoveryDogModel candidate = Candidate("Lua");
        TaskCompletionSource<DiscoveryPageModel> pageGate =
            NewCompletionSource<DiscoveryPageModel>();
        TaskCompletionSource pageStarted =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        api.GetDogsAsync(Arg.Any<CancellationToken>()).Returns([source]);
        api.DiscoverAsync(source.Id, Arg.Any<DiscoveryFilterModel>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                if (call.ArgAt<DiscoveryFilterModel>(1)!.Page == 1)
                {
                    return Task.FromResult(Page([candidate], hasMore: true));
                }

                pageStarted.TrySetResult();
                return pageGate.Task.WaitAsync(call.Arg<CancellationToken>());
            });
        DiscoveryViewModel viewModel = new(api);
        await viewModel.ActivateAsync(TestCancellation.Token);

        Task pagination = viewModel.LoadMoreCommand.ExecuteAsync(null);
        await pageStarted.Task.WaitAsync(TestCancellation.Token);

        viewModel.LikeCommand.CanExecute(candidate).Should().BeFalse();
        viewModel.PassCommand.CanExecute(candidate).Should().BeFalse();
        viewModel.BlockCommand.CanExecute(candidate).Should().BeFalse();
        await viewModel.LikeCommand.ExecuteAsync(candidate);
        await viewModel.PassCommand.ExecuteAsync(candidate);
        await viewModel.BlockCommand.ExecuteAsync(candidate);
        await api.DidNotReceive().LikeAsync(
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
        await api.DidNotReceive().PassAsync(
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
        await api.DidNotReceive().BlockDogOwnerAsync(
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());

        pageGate.SetResult(Page([Candidate("Thor")], page: 2));
        await pagination;

        viewModel.LikeCommand.CanExecute(candidate).Should().BeTrue();
        viewModel.PassCommand.CanExecute(candidate).Should().BeTrue();
        viewModel.BlockCommand.CanExecute(candidate).Should().BeTrue();
        viewModel.Candidates.Should().HaveCount(2);
    }

    [Fact]
    public async Task CandidateActionShouldPreventPaginationUntilItFinishes()
    {
        DogModel source = SourceDog("Mel");
        DiscoveryDogModel candidate = Candidate("Lua");
        TaskCompletionSource<LikeDogModel> likeGate =
            NewCompletionSource<LikeDogModel>();
        IPetMachApiClient api = CreateApi(
            [source],
            Page([candidate], hasMore: true));
        api.LikeAsync(source.Id, candidate.DogId, Arg.Any<CancellationToken>())
            .Returns(likeGate.Task);
        DiscoveryViewModel viewModel = new(api);
        await viewModel.ActivateAsync(TestCancellation.Token);

        Task action = viewModel.LikeCommand.ExecuteAsync(candidate);

        viewModel.LoadMoreCommand.CanExecute(null).Should().BeFalse();
        await viewModel.LoadMoreCommand.ExecuteAsync(null);
        await api.Received(1).DiscoverAsync(
            source.Id,
            Arg.Any<DiscoveryFilterModel>(),
            Arg.Any<CancellationToken>());

        likeGate.SetResult(new LikeDogModel(false, null));
        await action;
    }

    [Fact]
    public async Task LatePaginationShouldNotOverwriteAFilteredReload()
    {
        DogModel source = SourceDog("Mel");
        DiscoveryDogModel original = Candidate("Lua");
        DiscoveryDogModel filtered = Candidate("Nina");
        TaskCompletionSource<DiscoveryPageModel> pageGate =
            NewCompletionSource<DiscoveryPageModel>();
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        api.GetDogsAsync(Arg.Any<CancellationToken>()).Returns([source]);
        api.DiscoverAsync(source.Id, Arg.Any<DiscoveryFilterModel>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                DiscoveryFilterModel filter = call.ArgAt<DiscoveryFilterModel>(1)!;
                if (filter.Page == 2)
                {
                    return pageGate.Task;
                }

                return Task.FromResult(filter.Breed == "SRD filtrado"
                    ? Page([filtered])
                    : Page([original], hasMore: true));
            });
        DiscoveryViewModel viewModel = new(api);
        await viewModel.ActivateAsync(TestCancellation.Token);

        Task pagination = viewModel.LoadMoreCommand.ExecuteAsync(null);
        viewModel.BreedFilter = "SRD filtrado";
        await viewModel.RefreshCommand.ExecuteAsync(null);
        pageGate.SetResult(Page([Candidate("Resposta antiga")], page: 2));
        await pagination;

        viewModel.Candidates.Should().ContainSingle().Which.Should().Be(filtered);
        viewModel.ScreenState.Should().Be(DiscoveryScreenState.Content);
    }

    [Fact]
    public async Task LikeShouldRemoveOnlyTheSelectedCardAfterSuccess()
    {
        DogModel source = SourceDog("Mel");
        DiscoveryDogModel first = Candidate("Lua");
        DiscoveryDogModel second = Candidate("Thor");
        IPetMachApiClient api = CreateApi([source], Page([first, second]));
        api.LikeAsync(source.Id, first.DogId, Arg.Any<CancellationToken>())
            .Returns(new LikeDogModel(false, null));
        DiscoveryViewModel viewModel = new(api);
        await viewModel.ActivateAsync(TestCancellation.Token);

        await viewModel.LikeCommand.ExecuteAsync(first);

        viewModel.Candidates.Should().ContainSingle().Which.Should().Be(second);
        viewModel.FeedbackKind.Should().Be(DiscoveryFeedbackKind.Success);
        viewModel.StatusMessage.Should().Be("Curtida enviada.");
    }

    [Fact]
    public async Task LikeFailureShouldPreserveTheCandidateAndExposeErrorFeedback()
    {
        DogModel source = SourceDog("Mel");
        DiscoveryDogModel candidate = Candidate("Lua");
        IPetMachApiClient api = CreateApi([source], Page([candidate]));
        api.LikeAsync(source.Id, candidate.DogId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<LikeDogModel>(new HttpRequestException()));
        DiscoveryViewModel viewModel = new(api);
        await viewModel.ActivateAsync(TestCancellation.Token);

        await viewModel.LikeCommand.ExecuteAsync(candidate);

        viewModel.Candidates.Should().ContainSingle().Which.Should().Be(candidate);
        viewModel.FeedbackKind.Should().Be(DiscoveryFeedbackKind.Error);
        viewModel.StatusMessage.Should().Contain("preservado");
        viewModel.IsActionInProgress.Should().BeFalse();
    }

    [Fact]
    public async Task PassShouldRemoveTheCandidateOnlyAfterSuccess()
    {
        DogModel source = SourceDog("Mel");
        DiscoveryDogModel candidate = Candidate("Lua");
        IPetMachApiClient api = CreateApi([source], Page([candidate]));
        DiscoveryViewModel viewModel = new(api);
        await viewModel.ActivateAsync(TestCancellation.Token);

        await viewModel.PassCommand.ExecuteAsync(candidate);

        viewModel.Candidates.Should().BeEmpty();
        viewModel.ScreenState.Should().Be(DiscoveryScreenState.Empty);
        viewModel.FeedbackKind.Should().Be(DiscoveryFeedbackKind.Success);
        await api.Received(1).PassAsync(
            source.Id,
            candidate.DogId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PassFailureShouldPreserveTheCandidate()
    {
        DogModel source = SourceDog("Mel");
        DiscoveryDogModel candidate = Candidate("Lua");
        IPetMachApiClient api = CreateApi([source], Page([candidate]));
        api.PassAsync(source.Id, candidate.DogId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new HttpRequestException()));
        DiscoveryViewModel viewModel = new(api);
        await viewModel.ActivateAsync(TestCancellation.Token);

        await viewModel.PassCommand.ExecuteAsync(candidate);

        viewModel.Candidates.Should().ContainSingle().Which.Should().Be(candidate);
        viewModel.FeedbackKind.Should().Be(DiscoveryFeedbackKind.Error);
    }

    [Fact]
    public async Task BlockShouldRemoveTheCandidateAfterSuccess()
    {
        DogModel source = SourceDog("Mel");
        DiscoveryDogModel candidate = Candidate("Lua");
        IPetMachApiClient api = CreateApi([source], Page([candidate]));
        DiscoveryViewModel viewModel = new(api);
        await viewModel.ActivateAsync(TestCancellation.Token);

        await viewModel.BlockCommand.ExecuteAsync(candidate);

        viewModel.Candidates.Should().BeEmpty();
        viewModel.StatusMessage.Should().Contain("Tutor bloqueado");
        await api.Received(1).BlockDogOwnerAsync(
            candidate.DogId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BlockFailureShouldPreserveTheCandidate()
    {
        DogModel source = SourceDog("Mel");
        DiscoveryDogModel candidate = Candidate("Lua");
        IPetMachApiClient api = CreateApi([source], Page([candidate]));
        api.BlockDogOwnerAsync(candidate.DogId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new HttpRequestException()));
        DiscoveryViewModel viewModel = new(api);
        await viewModel.ActivateAsync(TestCancellation.Token);

        await viewModel.BlockCommand.ExecuteAsync(candidate);

        viewModel.Candidates.Should().ContainSingle().Which.Should().Be(candidate);
        viewModel.FeedbackKind.Should().Be(DiscoveryFeedbackKind.Error);
        viewModel.IsActionInProgress.Should().BeFalse();
    }

    [Fact]
    public async Task ConcurrentCandidateActionsShouldSendOnlyTheFirstAction()
    {
        DogModel source = SourceDog("Mel");
        DiscoveryDogModel candidate = Candidate("Lua");
        TaskCompletionSource<LikeDogModel> likeGate =
            NewCompletionSource<LikeDogModel>();
        IPetMachApiClient api = CreateApi([source], Page([candidate]));
        api.LikeAsync(source.Id, candidate.DogId, Arg.Any<CancellationToken>())
            .Returns(likeGate.Task);
        DiscoveryViewModel viewModel = new(api);
        await viewModel.ActivateAsync(TestCancellation.Token);

        Task like = viewModel.LikeCommand.ExecuteAsync(candidate);
        Task pass = viewModel.PassCommand.ExecuteAsync(candidate);
        likeGate.SetResult(new LikeDogModel(false, null));
        await Task.WhenAll(like, pass);

        await api.Received(1).LikeAsync(
            source.Id,
            candidate.DogId,
            Arg.Any<CancellationToken>());
        await api.DidNotReceive().PassAsync(
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LateCandidateActionShouldNotRemoveCandidateFromANewDogContext()
    {
        DogModel firstSource = SourceDog("Mel");
        DogModel secondSource = SourceDog("Thor");
        DiscoveryDogModel firstCandidate = Candidate("Lua");
        DiscoveryDogModel secondCandidate = Candidate("Nina");
        TaskCompletionSource<LikeDogModel> likeGate =
            NewCompletionSource<LikeDogModel>();
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        api.GetDogsAsync(Arg.Any<CancellationToken>()).Returns([firstSource, secondSource]);
        api.DiscoverAsync(
                Arg.Any<Guid>(),
                Arg.Any<DiscoveryFilterModel>(),
                Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Guid>() == firstSource.Id
                ? Page([firstCandidate])
                : Page([secondCandidate]));
        api.LikeAsync(firstSource.Id, firstCandidate.DogId, Arg.Any<CancellationToken>())
            .Returns(likeGate.Task);
        DiscoveryViewModel viewModel = new(api);
        await viewModel.ActivateAsync(TestCancellation.Token);

        Task action = viewModel.LikeCommand.ExecuteAsync(firstCandidate);
        await viewModel.SelectDogAsync(secondSource, TestCancellation.Token);
        likeGate.SetResult(new LikeDogModel(false, null));
        await action;

        viewModel.SelectedDog.Should().Be(secondSource);
        viewModel.Candidates.Should().ContainSingle().Which.Should().Be(secondCandidate);
        viewModel.FeedbackKind.Should().Be(DiscoveryFeedbackKind.None);
    }

    [Fact]
    public async Task NoSourceDogShouldExposeEmptyStateWithoutCallingDiscovery()
    {
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        api.GetDogsAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<DogModel>());
        DiscoveryViewModel viewModel = new(api);

        await viewModel.ActivateAsync(TestCancellation.Token);

        viewModel.SelectedDog.Should().BeNull();
        viewModel.ScreenState.Should().Be(DiscoveryScreenState.Empty);
        viewModel.EmptyMessage.Should().Contain("Cadastre um cão");
        await api.DidNotReceive().DiscoverAsync(
            Arg.Any<Guid>(),
            Arg.Any<DiscoveryFilterModel>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MutualLikeShouldRemainTextualFeedback()
    {
        DogModel source = SourceDog("Mel");
        DiscoveryDogModel candidate = Candidate("Lua");
        IPetMachApiClient api = CreateApi([source], Page([candidate]));
        api.LikeAsync(source.Id, candidate.DogId, Arg.Any<CancellationToken>())
            .Returns(new LikeDogModel(true, Guid.NewGuid()));
        DiscoveryViewModel viewModel = new(api);
        await viewModel.ActivateAsync(TestCancellation.Token);

        await viewModel.LikeCommand.ExecuteAsync(candidate);

        viewModel.StatusMessage.Should().Contain("match")
            .And.Contain("conversar")
            .And.Contain("encontro");
        viewModel.FeedbackKind.Should().Be(DiscoveryFeedbackKind.Success);
    }

    [Fact]
    public async Task ReloadShouldClearPreviousActionError()
    {
        DogModel source = SourceDog("Mel");
        DiscoveryDogModel candidate = Candidate("Lua");
        IPetMachApiClient api = CreateApi([source], Page([candidate]));
        api.LikeAsync(source.Id, candidate.DogId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<LikeDogModel>(new HttpRequestException()));
        DiscoveryViewModel viewModel = new(api);
        await viewModel.ActivateAsync(TestCancellation.Token);
        await viewModel.LikeCommand.ExecuteAsync(candidate);
        viewModel.FeedbackKind.Should().Be(DiscoveryFeedbackKind.Error);

        await viewModel.RefreshCommand.ExecuteAsync(null);

        viewModel.FeedbackKind.Should().Be(DiscoveryFeedbackKind.None);
        viewModel.StatusMessage.Should().BeEmpty();
        viewModel.ScreenState.Should().Be(DiscoveryScreenState.Content);
    }

    private static IPetMachApiClient CreateApi(
        IReadOnlyCollection<DogModel> dogs,
        DiscoveryPageModel page)
    {
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        api.GetDogsAsync(Arg.Any<CancellationToken>()).Returns(dogs);
        api.DiscoverAsync(
                Arg.Any<Guid>(),
                Arg.Any<DiscoveryFilterModel>(),
                Arg.Any<CancellationToken>())
            .Returns(page);
        return api;
    }

    private static TaskCompletionSource<T> NewCompletionSource<T>() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static DiscoveryPageModel Page(
        IReadOnlyCollection<DiscoveryDogModel> candidates,
        int page = 1,
        bool hasMore = false) =>
        new(candidates, page, 10, hasMore);

    private static DogModel SourceDog(string name) => new(
        Guid.NewGuid(),
        name,
        null,
        true,
        DogSexModel.Female,
        "SRD",
        DogSizeModel.Medium,
        null,
        true,
        "Dócil",
        EnergyLevelModel.Moderate,
        5,
        5,
        5,
        null,
        null,
        null,
        DogGoalModel.Friendship,
        DogProfileStatusModel.Active);

    private static DiscoveryDogModel Candidate(string name) => new(
        Guid.NewGuid(),
        name,
        null,
        true,
        DogSexModel.Female,
        "SRD",
        DogSizeModel.Medium,
        "Amigável",
        EnergyLevelModel.Moderate,
        DogGoalModel.Friendship,
        true,
        true,
        "Lisboa",
        null);
}
