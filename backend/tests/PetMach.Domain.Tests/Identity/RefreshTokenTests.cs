using FluentAssertions;
using PetMach.Domain.Identity;

namespace PetMach.Domain.Tests.Identity;

public sealed class RefreshTokenTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 14, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void IssueShouldCreateActiveToken()
    {
        RefreshToken token = RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), new string('A', 64), Now, Now.AddDays(30)).Value;

        token.IsActive(Now).Should().BeTrue();
    }

    [Fact]
    public void ConsumedTokenCannotBeConsumedTwice()
    {
        RefreshToken token = RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), new string('A', 64), Now, Now.AddDays(30)).Value;

        token.Consume(Guid.NewGuid(), Now).IsSuccess.Should().BeTrue();
        token.Consume(Guid.NewGuid(), Now.AddSeconds(1)).IsFailure.Should().BeTrue();
        token.IsActive(Now.AddSeconds(1)).Should().BeFalse();
    }

    [Fact]
    public void ExpiredTokenShouldNotBeActive()
    {
        RefreshToken token = RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), new string('A', 64), Now, Now.AddMinutes(1)).Value;

        token.IsActive(Now.AddMinutes(1)).Should().BeFalse();
    }
}
