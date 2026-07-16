using FluentAssertions;
using PetMach.Domain.Partners;

namespace PetMach.Domain.Tests.Partners;

public sealed class PartnerDomainTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 15, 18, 0, 0, TimeSpan.Zero);

    [Fact]
    public void PartnerShouldNormalizeState()
    {
        PartnerEstablishment partner = new(Guid.NewGuid(), "Pet Legal Ltda", "Pet Legal", "12345678000100", "Lisboa", "pt", "Europe/Lisbon", Now);
        partner.State.Should().Be("PT");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1001)]
    public void SpaceShouldRejectInvalidCapacity(int capacity)
    {
        Action create = () => _ = new PartnerSpace(Guid.NewGuid(), "Parque", "Espaço aberto", capacity, 10, Now);
        create.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SpaceShouldRejectNegativePrice()
    {
        Action create = () => _ = new PartnerSpace(Guid.NewGuid(), "Parque", "Espaço aberto", 10, -1, Now);
        create.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AvailabilityShouldNormalizeDatesAndClose()
    {
        SpaceAvailability availability = new(Guid.NewGuid(), Now.AddHours(2).ToOffset(TimeSpan.FromHours(1)), Now.AddHours(4).ToOffset(TimeSpan.FromHours(1)), Now);
        availability.StartsAtUtc.Offset.Should().Be(TimeSpan.Zero);
        availability.EndsAtUtc.Offset.Should().Be(TimeSpan.Zero);
        availability.IsAvailable.Should().BeTrue();
        availability.Close();
        availability.IsAvailable.Should().BeFalse();
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(2, 1)]
    [InlineData(1, 170)]
    public void AvailabilityShouldRejectInvalidPeriod(int startHours, int endHours)
    {
        Action create = () => _ = new SpaceAvailability(Guid.NewGuid(), Now.AddHours(startHours), Now.AddHours(endHours), Now);
        create.Should().Throw<ArgumentException>();
    }
}
