using FluentAssertions;
using PetMach.Domain.Adoption;

namespace PetMach.Domain.Tests.Adoption;

public sealed class AdoptionApplicationTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 16, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ApplicationShouldFollowReviewAndApprovalFlow()
    {
        AdoptionApplication application = Create();
        application.Status.Should().Be(AdoptionApplicationStatus.Submitted);
        application.StartReview(Now.AddMinutes(1));
        application.Approve(Now.AddMinutes(2));
        application.Status.Should().Be(AdoptionApplicationStatus.Approved);
    }

    [Fact]
    public void ApplicationShouldRejectApprovalBeforeReview()
    {
        AdoptionApplication application = Create();
        Action approve = () => application.Approve(Now.AddMinutes(1));
        approve.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ApplicantShouldWithdrawSubmittedApplication()
    {
        AdoptionApplication application = Create();
        application.Withdraw(Now.AddMinutes(1));
        application.Status.Should().Be(AdoptionApplicationStatus.Withdrawn);
    }

    [Fact]
    public void HistoryShouldRejectEmptyActor()
    {
        Action create = () => _ = new AdoptionApplicationHistory(Guid.NewGuid(), Guid.Empty, null, AdoptionApplicationStatus.Submitted, Now);
        create.Should().Throw<ArgumentException>();
    }

    private static AdoptionApplication Create() => new(Guid.NewGuid(), Guid.NewGuid(), "Quero oferecer um lar responsável", "Experiência com cães", "Apartamento seguro", "2026-07-16", Now);
}
