using FluentAssertions;
using PetMach.Domain.Adoption;

namespace PetMach.Domain.Tests.Adoption;

public sealed class AdoptionProfileTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 16, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ProfileShouldStartAvailableWithAcceptedTerms()
    {
        AdoptionProfile profile = new(Guid.NewGuid(), Guid.NewGuid(), "História responsável", "Lar seguro e acompanhamento", "2026-07-16", Now);
        profile.Status.Should().Be(AdoptionStatus.Available);
        profile.TermsAcceptedAtUtc.Should().Be(Now);
    }

    [Fact]
    public void ProfileShouldRejectMissingStory()
    {
        Action create = () => _ = new AdoptionProfile(Guid.NewGuid(), Guid.NewGuid(), " ", "Lar seguro", "2026-07-16", Now);
        create.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AvailableProfileShouldSuspend()
    {
        AdoptionProfile profile = new(Guid.NewGuid(), Guid.NewGuid(), "História", "Requisitos", "2026-07-16", Now);
        profile.Suspend(Now.AddMinutes(1));
        profile.Status.Should().Be(AdoptionStatus.Suspended);
        Action suspendAgain = () => profile.Suspend(Now.AddMinutes(2));
        suspendAgain.Should().Throw<InvalidOperationException>();
    }
}
