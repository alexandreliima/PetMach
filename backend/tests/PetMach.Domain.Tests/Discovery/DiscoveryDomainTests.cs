using FluentAssertions;
using PetMach.Domain.Discovery;
using PetMach.Domain.Matches;
using PetMach.Domain.Moderation;
using PetMach.Domain.Notifications;

namespace PetMach.Domain.Tests.Discovery;

public sealed class DiscoveryDomainTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 14, 14, 0, 0, TimeSpan.Zero);

    [Fact]
    public void LikeShouldRejectTheSameDog()
    {
        Guid dogId = Guid.NewGuid();
        Action create = () => _ = new DogLike(dogId, dogId, Now);
        create.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MatchShouldOrderDogsAndEndOnlyOnce()
    {
        Guid first = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
        Guid second = Guid.Parse("11111111-1111-1111-1111-111111111111");
        DogMatch match = new(first, second, Now);

        match.DogAId.Should().Be(second);
        match.DogBId.Should().Be(first);
        match.End(Now.AddHours(1));
        match.End(Now.AddHours(2));
        match.EndedAtUtc.Should().Be(Now.AddHours(1));
    }

    [Fact]
    public void BlockingSelfShouldBeRejected()
    {
        Guid userId = Guid.NewGuid();
        Action create = () => _ = new BlockedUser(userId, userId, Now);
        create.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NotificationReadShouldBeMonotonic()
    {
        UserNotification notification = new(Guid.NewGuid(), Guid.NewGuid(), "Novo match", "Dois cães combinaram.", Now);

        notification.MarkAsRead(Now.AddMinutes(1));
        notification.MarkAsRead(Now.AddMinutes(2));

        notification.ReadAtUtc.Should().Be(Now.AddMinutes(1));
    }
}
