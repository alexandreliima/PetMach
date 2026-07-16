namespace PetMach.Domain.Reservations;

public enum ReservationStatus { Pending, Confirmed, Cancelled, Completed, NoShow }
public enum ReservationPaymentStatus { AwaitingOnSite, RecordedOnSite }

public sealed class Reservation
{
    private Reservation() { }

    public Reservation(Guid availabilityId, Guid requesterUserId, Guid dogId, DateTimeOffset now)
    {
        if (availabilityId == Guid.Empty || requesterUserId == Guid.Empty || dogId == Guid.Empty) throw new ArgumentException("Reserva inválida.");
        Id = Guid.NewGuid(); AvailabilityId = availabilityId; RequesterUserId = requesterUserId; DogId = dogId;
        Status = ReservationStatus.Pending; PaymentStatus = ReservationPaymentStatus.AwaitingOnSite; CreatedAtUtc = now.ToUniversalTime(); UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid AvailabilityId { get; private set; }
    public Guid RequesterUserId { get; private set; }
    public Guid DogId { get; private set; }
    public ReservationStatus Status { get; private set; }
    public ReservationPaymentStatus PaymentStatus { get; private set; }
    public Guid? CancelledByUserId { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public void Confirm(DateTimeOffset now)
    {
        if (Status != ReservationStatus.Pending) throw new InvalidOperationException("Somente reservas pendentes podem ser confirmadas.");
        Status = ReservationStatus.Confirmed; UpdatedAtUtc = now.ToUniversalTime();
    }

    public void Cancel(Guid actorUserId, DateTimeOffset now)
    {
        if (actorUserId == Guid.Empty || Status is not (ReservationStatus.Pending or ReservationStatus.Confirmed)) throw new InvalidOperationException("A reserva não pode ser cancelada.");
        Status = ReservationStatus.Cancelled; CancelledByUserId = actorUserId; CancelledAtUtc = now.ToUniversalTime(); UpdatedAtUtc = CancelledAtUtc.Value;
    }

    public void Complete(bool paymentReceivedOnSite, DateTimeOffset now)
    {
        if (Status != ReservationStatus.Confirmed) throw new InvalidOperationException("Somente reservas confirmadas podem ser concluídas.");
        Status = ReservationStatus.Completed;
        if (paymentReceivedOnSite) PaymentStatus = ReservationPaymentStatus.RecordedOnSite;
        UpdatedAtUtc = now.ToUniversalTime();
    }

    public void MarkNoShow(DateTimeOffset now)
    {
        if (Status != ReservationStatus.Confirmed) throw new InvalidOperationException("Somente reservas confirmadas podem registrar ausência.");
        Status = ReservationStatus.NoShow; UpdatedAtUtc = now.ToUniversalTime();
    }
}
