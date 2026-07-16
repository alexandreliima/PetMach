namespace PetMach.Contracts.Reservations;

public sealed record CreateReservationRequest(Guid AvailabilityId, Guid DogId);
public sealed record CompleteReservationRequest(bool PaymentReceivedOnSite);
public sealed record ReservationResponse(Guid Id, Guid AvailabilityId, Guid SpaceId, string SpaceName, Guid DogId, string DogName, DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc, string Status, string PaymentStatus, DateTimeOffset CreatedAtUtc, DateTimeOffset? CancelledAtUtc);
public sealed record ReservationHistoryResponse(Guid Id, string? FromStatus, string ToStatus, string Action, DateTimeOffset OccurredAtUtc);
