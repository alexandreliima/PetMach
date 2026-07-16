using FluentAssertions;
using PetMach.Domain.Reservations;

namespace PetMach.Domain.Tests.Reservations;

public sealed class ReservationTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 15, 20, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ReservationShouldStartPendingAndConfirm()
    {
        Reservation reservation = new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Now);
        reservation.Status.Should().Be(ReservationStatus.Pending);
        reservation.Confirm(Now.AddMinutes(1));
        reservation.Status.Should().Be(ReservationStatus.Confirmed);
        reservation.UpdatedAtUtc.Should().Be(Now.AddMinutes(1));
    }

    [Fact]
    public void ConfirmShouldRejectNonPendingReservation()
    {
        Reservation reservation = new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Now);
        reservation.Confirm(Now.AddMinutes(1));
        Action confirmAgain = () => reservation.Confirm(Now.AddMinutes(2));
        confirmAgain.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ReservationShouldRejectEmptyReferences()
    {
        Action create = () => _ = new Reservation(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), Now);
        create.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void PendingReservationShouldCancelWithActorAndTime()
    {
        Guid actorId = Guid.NewGuid();
        Reservation reservation = new(Guid.NewGuid(), actorId, Guid.NewGuid(), Now);
        reservation.Cancel(actorId, Now.AddMinutes(5));
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
        reservation.CancelledByUserId.Should().Be(actorId);
        reservation.CancelledAtUtc.Should().Be(Now.AddMinutes(5));
    }

    [Fact]
    public void ConfirmedReservationShouldCompleteAndRecordOnsitePayment()
    {
        Reservation reservation = new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Now);
        reservation.Confirm(Now.AddMinutes(1));
        reservation.Complete(true, Now.AddHours(2));
        reservation.Status.Should().Be(ReservationStatus.Completed);
        reservation.PaymentStatus.Should().Be(ReservationPaymentStatus.RecordedOnSite);
    }

    [Fact]
    public void PendingReservationShouldNotComplete()
    {
        Reservation reservation = new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Now);
        Action complete = () => reservation.Complete(false, Now.AddHours(2));
        complete.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void HistoryShouldRejectEmptyActor()
    {
        Action create = () => _ = new ReservationHistoryEntry(Guid.NewGuid(), Guid.Empty, null, ReservationStatus.Pending, "Created", Now);
        create.Should().Throw<ArgumentException>();
    }
}
