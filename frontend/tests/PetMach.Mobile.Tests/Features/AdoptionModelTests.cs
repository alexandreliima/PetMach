using FluentAssertions;
using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile.Tests.Features;

public sealed class AdoptionModelTests
{
    [Fact]
    public void AvailableForeignProfileShouldAllowApplication()
    {
        AdoptionProfileModel profile = new(Guid.NewGuid(), Guid.NewGuid(), "Luna", "SRD", "Medium", "Lisboa", "História", "Requisitos", "Available", DateTimeOffset.UtcNow, false);
        profile.CanApply.Should().BeTrue();
        profile.CanSuspend.Should().BeFalse();
    }

    [Theory]
    [InlineData("Submitted", true)]
    [InlineData("UnderReview", true)]
    [InlineData("Approved", false)]
    [InlineData("Rejected", false)]
    public void ApplicationShouldExposeWithdrawalState(string status, bool canWithdraw)
    {
        AdoptionApplicationModel application = new(Guid.NewGuid(), Guid.NewGuid(), "Luna", "Ana", "Motivação", "Experiência", "Lar", status, DateTimeOffset.UtcNow, true);
        application.CanWithdraw.Should().Be(canWithdraw);
    }
}
