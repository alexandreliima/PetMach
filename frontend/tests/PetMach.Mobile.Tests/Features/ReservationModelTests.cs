using FluentAssertions;
using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile.Tests.Features;

public sealed class ReservationModelTests
{
    [Theory]
    [InlineData("Pending", true, "Aguardando confirmação")]
    [InlineData("Confirmed", true, "Confirmada")]
    [InlineData("Cancelled", false, "Cancelada")]
    [InlineData("Completed", false, "Concluída")]
    public void ReservationShouldExposePresentationState(string status, bool canCancel, string label)
    {
        ReservationModel reservation = new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Parque", Guid.NewGuid(), "Luna", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1), status, "AwaitingOnSite", DateTimeOffset.UtcNow, null);
        reservation.CanCancel.Should().Be(canCancel);
        reservation.StatusLabel.Should().Be(label);
    }
}
