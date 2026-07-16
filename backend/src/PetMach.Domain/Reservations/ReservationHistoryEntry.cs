namespace PetMach.Domain.Reservations;

public sealed class ReservationHistoryEntry
{
    private ReservationHistoryEntry() { }
    public ReservationHistoryEntry(Guid reservationId, Guid actorUserId, ReservationStatus? fromStatus, ReservationStatus toStatus, string action, DateTimeOffset occurredAtUtc)
    {
        if (reservationId == Guid.Empty || actorUserId == Guid.Empty || string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Histórico de reserva inválido.");
        Id = Guid.NewGuid(); ReservationId = reservationId; ActorUserId = actorUserId; FromStatus = fromStatus; ToStatus = toStatus;
        Action = action.Trim(); OccurredAtUtc = occurredAtUtc.ToUniversalTime();
    }
    public Guid Id { get; private set; }
    public Guid ReservationId { get; private set; }
    public Guid ActorUserId { get; private set; }
    public ReservationStatus? FromStatus { get; private set; }
    public ReservationStatus ToStatus { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; private set; }
}
