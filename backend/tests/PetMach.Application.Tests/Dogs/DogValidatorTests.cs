using FluentAssertions;
using PetMach.Application.Dogs;
using PetMach.Contracts.Dogs;

namespace PetMach.Application.Tests.Dogs;

public sealed class DogValidatorTests
{
    [Fact]
    public void ShouldAcceptACompleteDogProfile()
    {
        UpsertDogRequest request = new("Pingo", null, true, DogSexContract.Male, "SRD", DogSizeContract.Medium, 14, true, "Brincalhão", EnergyLevelContract.High, 4, 5, 3, null, null, null, DogGoalContract.Walks);

        new DogValidator().Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldRejectAnInvalidWeightAndScore()
    {
        UpsertDogRequest request = new("Pingo", null, true, DogSexContract.Male, "SRD", DogSizeContract.Medium, 0, true, "Brincalhão", EnergyLevelContract.High, 0, 5, 3, null, null, null, DogGoalContract.Walks);

        new DogValidator().Validate(request).IsValid.Should().BeFalse();
    }
}
