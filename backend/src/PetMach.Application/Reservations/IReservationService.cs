using PetMach.Contracts.Reservations;
using PetMach.Domain.SharedKernel;

namespace PetMach.Application.Reservations;

public interface IReservationService
{
    Task<Result<ReservationResponse>> CreateAsync(Guid requesterUserId, CreateReservationRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ReservationResponse>> ListForTutorAsync(Guid requesterUserId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ReservationResponse>> ListForPartnerAsync(Guid ownerUserId, CancellationToken cancellationToken);
    Task<Result<ReservationResponse>> ConfirmAsync(Guid ownerUserId, Guid reservationId, CancellationToken cancellationToken);
    Task<Result<ReservationResponse>> CancelForTutorAsync(Guid requesterUserId, Guid reservationId, CancellationToken cancellationToken);
    Task<Result<ReservationResponse>> CancelForPartnerAsync(Guid ownerUserId, Guid reservationId, CancellationToken cancellationToken);
    Task<Result<ReservationResponse>> CompleteAsync(Guid ownerUserId, Guid reservationId, bool paymentReceivedOnSite, CancellationToken cancellationToken);
    Task<Result<ReservationResponse>> MarkNoShowAsync(Guid ownerUserId, Guid reservationId, CancellationToken cancellationToken);
    Task<Result<IReadOnlyCollection<ReservationHistoryResponse>>> HistoryAsync(Guid userId, bool isPartner, Guid reservationId, CancellationToken cancellationToken);
}
