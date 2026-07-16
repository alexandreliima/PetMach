using FluentAssertions;
using PetMach.Domain.Tutors;

namespace PetMach.Domain.Tests.Tutors;

public sealed class TutorProfileTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 14, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateShouldNormalizeStateAndRespectPrivacyChoices()
    {
        TutorProfile profile = TutorProfile.Create(Guid.NewGuid(), "Alex", "Lima", null, "Lisboa", "pt", "Tutor responsável", false, true, Now).Value;

        profile.State.Should().Be("PT");
        profile.ShowCity.Should().BeFalse();
        profile.AllowDiscovery.Should().BeTrue();
    }

    [Fact]
    public void CreateShouldRejectMissingRequiredName()
    {
        TutorProfile.Create(Guid.NewGuid(), string.Empty, "Lima", null, "Lisboa", "PT", null, false, false, Now)
            .IsFailure.Should().BeTrue();
    }
}
