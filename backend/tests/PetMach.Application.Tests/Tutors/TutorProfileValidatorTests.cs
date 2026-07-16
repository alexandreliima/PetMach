using FluentAssertions;
using PetMach.Application.Tutors;
using PetMach.Contracts.Tutors;

namespace PetMach.Application.Tests.Tutors;

public sealed class TutorProfileValidatorTests
{
    [Fact]
    public void ValidatorShouldAcceptValidProfile()
    {
        UpsertTutorProfileRequest request = new("Alex", "Lima", null, "Lisboa", "PT", "Tutor responsável", false, true);

        new TutorProfileValidator().Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidatorShouldRejectOversizedBiography()
    {
        UpsertTutorProfileRequest request = new("Alex", "Lima", null, "Lisboa", "PT", new string('A', 1001), false, true);

        new TutorProfileValidator().Validate(request).IsValid.Should().BeFalse();
    }
}
