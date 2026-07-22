using FluentAssertions;
using NSubstitute;
using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile.Tests.Features;

public sealed class DiscoveryPageLifecycleTests
{
    [Fact]
    public async Task ConsecutiveAppearShouldShareTheActiveLifecycleAndAllowLaterReloads()
    {
        DogModel firstSource = SourceDog("Mel");
        DogModel secondSource = SourceDog("Thor");
        TaskCompletionSource<IReadOnlyCollection<DogModel>> dogsGate =
            NewCompletionSource<IReadOnlyCollection<DogModel>>();
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        api.GetDogsAsync(Arg.Any<CancellationToken>())
            .Returns(dogsGate.Task);
        api.DiscoverAsync(
                Arg.Any<Guid>(),
                Arg.Any<DiscoveryFilterModel>(),
                Arg.Any<CancellationToken>())
            .Returns(Page([Candidate("Lua")]));
        DiscoveryViewModel viewModel = new(api);
        using DiscoveryPageLifecycle lifecycle = new(viewModel);

        Task firstAppearance = lifecycle.AppearAsync();
        CancellationToken firstToken = lifecycle.CurrentToken!.Value;
        Task secondAppearance = lifecycle.AppearAsync();

        secondAppearance.Should().BeSameAs(firstAppearance);
        lifecycle.CurrentToken.Should().Be(firstToken);
        await api.Received(1).GetDogsAsync(Arg.Any<CancellationToken>());
        dogsGate.SetResult([firstSource, secondSource]);
        await Task.WhenAll(firstAppearance, secondAppearance);

        Func<Task> switchDog = () => viewModel.SelectDogAsync(
            secondSource,
            lifecycle.CurrentToken!.Value);
        await switchDog.Should().NotThrowAsync<ObjectDisposedException>();
        Func<Task> refresh = () => viewModel.RefreshCommand.ExecuteAsync(null);
        await refresh.Should().NotThrowAsync<ObjectDisposedException>();
        await api.Received(3).DiscoverAsync(
            Arg.Any<Guid>(),
            Arg.Any<DiscoveryFilterModel>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DisappearThenAppearShouldCancelThePreviousLifecycleAndCreateANewOne()
    {
        DogModel source = SourceDog("Mel");
        TaskCompletionSource<IReadOnlyCollection<DogModel>> firstDogs =
            NewCompletionSource<IReadOnlyCollection<DogModel>>();
        int dogRequests = 0;
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        api.GetDogsAsync(Arg.Any<CancellationToken>()).Returns(_ =>
        {
            dogRequests++;
            return dogRequests == 1
                ? firstDogs.Task
                : Task.FromResult<IReadOnlyCollection<DogModel>>([source]);
        });
        api.DiscoverAsync(
                source.Id,
                Arg.Any<DiscoveryFilterModel>(),
                Arg.Any<CancellationToken>())
            .Returns(Page([]));
        DiscoveryViewModel viewModel = new(api);
        using DiscoveryPageLifecycle lifecycle = new(viewModel);

        Task firstAppearance = lifecycle.AppearAsync();
        CancellationToken firstToken = lifecycle.CurrentToken!.Value;
        lifecycle.Disappear();

        firstToken.IsCancellationRequested.Should().BeTrue();
        lifecycle.IsActive.Should().BeFalse();
        viewModel.IsActive.Should().BeFalse();
        viewModel.ScreenState.Should().Be(DiscoveryScreenState.Initial);
        firstDogs.SetResult([SourceDog("Resposta antiga")]);
        await firstAppearance;
        viewModel.MyDogs.Should().BeEmpty();

        Task secondAppearance = lifecycle.AppearAsync();
        CancellationToken secondToken = lifecycle.CurrentToken!.Value;
        await secondAppearance;

        secondToken.Should().NotBe(firstToken);
        secondToken.IsCancellationRequested.Should().BeFalse();
        viewModel.IsActive.Should().BeTrue();
        viewModel.ScreenState.Should().Be(DiscoveryScreenState.Empty);
        await api.Received(2).GetDogsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DisappearDuringActionShouldIgnoreTheLateResponseAndPreserveTheCandidate()
    {
        DogModel source = SourceDog("Mel");
        DiscoveryDogModel candidate = Candidate("Lua");
        TaskCompletionSource<LikeDogModel> likeGate =
            NewCompletionSource<LikeDogModel>();
        CancellationToken actionToken = default;
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        api.GetDogsAsync(Arg.Any<CancellationToken>()).Returns([source]);
        api.DiscoverAsync(
                source.Id,
                Arg.Any<DiscoveryFilterModel>(),
                Arg.Any<CancellationToken>())
            .Returns(Page([candidate]));
        api.LikeAsync(source.Id, candidate.DogId, Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                actionToken = call.Arg<CancellationToken>();
                return likeGate.Task;
            });
        DiscoveryViewModel viewModel = new(api);
        using DiscoveryPageLifecycle lifecycle = new(viewModel);
        await lifecycle.AppearAsync();

        Task action = viewModel.LikeCommand.ExecuteAsync(candidate);
        lifecycle.Disappear();

        actionToken.IsCancellationRequested.Should().BeTrue();
        viewModel.ScreenState.Should().Be(DiscoveryScreenState.Initial);
        likeGate.SetResult(new LikeDogModel(false, null));
        await action;

        viewModel.Candidates.Should().ContainSingle().Which.Should().Be(candidate);
        viewModel.FeedbackKind.Should().Be(DiscoveryFeedbackKind.None);
        viewModel.ScreenState.Should().Be(DiscoveryScreenState.Initial);
    }

    private static TaskCompletionSource<T> NewCompletionSource<T>() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static DiscoveryPageModel Page(
        IReadOnlyCollection<DiscoveryDogModel> candidates) =>
        new(candidates, 1, 10, false);

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
