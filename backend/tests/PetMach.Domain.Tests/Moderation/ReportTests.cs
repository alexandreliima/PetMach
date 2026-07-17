using FluentAssertions;
using PetMach.Domain.Moderation;

namespace PetMach.Domain.Tests.Moderation;

public sealed class ReportTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 16, 14, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ReportShouldFollowReviewAndDismissFlow()
    {
        Report report = new(Guid.NewGuid(), ReportTargetType.Dog, Guid.NewGuid(), ReportReason.AnimalWelfare, "Condição insegura observada.", Now);
        report.Status.Should().Be(ReportStatus.Submitted);
        Guid moderatorId = Guid.NewGuid();
        report.StartReview(moderatorId, Now.AddMinutes(1));
        report.Dismiss(moderatorId, Now.AddMinutes(2));
        report.Status.Should().Be(ReportStatus.Dismissed);
        report.ReviewedByUserId.Should().Be(moderatorId);
    }

    [Fact]
    public void ReportShouldRejectEmptyDescription()
    {
        Action create = () => _ = new Report(Guid.NewGuid(), ReportTargetType.User, Guid.NewGuid(), ReportReason.Other, " ", Now);
        create.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EvidenceShouldRejectEmptyStorageKey()
    {
        Action create = () => _ = new ReportEvidence(Guid.NewGuid(), string.Empty, "image/jpeg", 100, Now);
        create.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ReviewedReportShouldBecomeActioned()
    {
        Report report = new(Guid.NewGuid(), ReportTargetType.User, Guid.NewGuid(), ReportReason.Fraud, "Indícios verificáveis.", Now);
        Guid moderatorId = Guid.NewGuid();
        report.StartReview(moderatorId, Now.AddMinutes(1));
        report.MarkActioned(moderatorId, Now.AddMinutes(2));
        report.Status.Should().Be(ReportStatus.Actioned);
        ModerationAction action = new(report.Id, moderatorId, ModerationActionType.SuspendUser, report.TargetType, report.TargetId, Now.AddMinutes(2));
        action.TargetId.Should().Be(report.TargetId);
    }
}
