using FluentAssertions;
using PetMach.Domain.Dogs;

namespace PetMach.Domain.Tests.Dogs;

public sealed class DogTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 14, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateShouldNormalizeFieldsAndStartAsDraft()
    {
        Dog dog = Dog.Create(Guid.NewGuid(), "  Pingo  ", new DateOnly(2022, 5, 1), false, DogSex.Male, "  SRD  ", DogSize.Medium, 14.5m, true, "  Brincalhão  ", EnergyLevel.High, 4, 5, 3, null, null, "  Adora parques  ", DogGoal.Walks, Now).Value;

        dog.Name.Should().Be("Pingo");
        dog.Breed.Should().Be("SRD");
        dog.Biography.Should().Be("Adora parques");
        dog.Status.Should().Be(DogProfileStatus.Draft);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void CreateShouldRejectInvalidSociabilityScore(int score)
    {
        Dog.Create(Guid.NewGuid(), "Pingo", null, true, DogSex.Male, "SRD", DogSize.Medium, null, false, "Brincalhão", EnergyLevel.High, score, 3, 3, null, null, null, DogGoal.Friendship, Now)
            .IsFailure.Should().BeTrue();
    }
}
