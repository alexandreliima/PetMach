using FluentAssertions;
using NSubstitute;
using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile.Tests.Features;

public sealed class DiscoveryViewModelTests
{
    [Fact]
    public async Task LikeShouldRemoveOnlyTheSelectedCardFromTheFeed()
    {
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        DogModel source = SourceDog();
        DiscoveryDogModel first = Candidate("Lua");
        DiscoveryDogModel second = Candidate("Thor");
        api.LikeAsync(source.Id, first.DogId, Arg.Any<CancellationToken>())
            .Returns(new LikeDogModel(false, null));
        DiscoveryViewModel viewModel = new(api) { SelectedDog = source };
        viewModel.Candidates.Add(first);
        viewModel.Candidates.Add(second);

        await viewModel.LikeCommand.ExecuteAsync(first);

        viewModel.Candidates.Should().ContainSingle().Which.Should().Be(second);
        viewModel.StatusMessage.Should().Be("Curtida enviada.");
    }

    [Fact]
    public async Task MutualLikeShouldExplainTheNextStep()
    {
        IPetMachApiClient api = Substitute.For<IPetMachApiClient>();
        DogModel source = SourceDog();
        DiscoveryDogModel candidate = Candidate("Lua");
        api.LikeAsync(source.Id, candidate.DogId, Arg.Any<CancellationToken>())
            .Returns(new LikeDogModel(true, Guid.NewGuid()));
        DiscoveryViewModel viewModel = new(api) { SelectedDog = source };
        viewModel.Candidates.Add(candidate);

        await viewModel.LikeCommand.ExecuteAsync(candidate);

        viewModel.StatusMessage.Should().Contain("conversar").And.Contain("encontro");
    }

    private static DogModel SourceDog() => new(
        Guid.NewGuid(),
        "Mel",
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
