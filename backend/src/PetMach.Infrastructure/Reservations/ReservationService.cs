using Microsoft.EntityFrameworkCore;
using PetMach.Application.Reservations;
using PetMach.Contracts.Reservations;
using PetMach.Domain.Reservations;
using PetMach.Domain.SharedKernel;
using PetMach.Infrastructure.Persistence;

namespace PetMach.Infrastructure.Reservations;

internal sealed class ReservationService(PetMachDbContext dbContext, TimeProvider timeProvider) : IReservationService
{
    private static readonly DomainError Invalid = new("reservations.invalid", "A disponibilidade ou o cão informado é inválido.");
    private static readonly DomainError Conflict = new("reservations.conflict", "Este horário já possui uma reserva ativa.");
    private static readonly DomainError NotFound = new("reservations.not_found", "Reserva não encontrada.");

    public async Task<Result<ReservationResponse>> CreateAsync(Guid requesterUserId, CreateReservationRequest request, CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        bool ownsDog = await dbContext.Dogs.AnyAsync(x => x.Id == request.DogId && x.OwnerUserId == requesterUserId, cancellationToken);
        bool validAvailability = await dbContext.SpaceAvailabilities.AnyAsync(x => x.Id == request.AvailabilityId && x.IsAvailable && x.StartsAtUtc > now, cancellationToken);
        if (!ownsDog || !validAvailability) return Result.Failure<ReservationResponse>(Invalid);
        bool occupied = await dbContext.Reservations.AnyAsync(x => x.AvailabilityId == request.AvailabilityId && (x.Status == ReservationStatus.Pending || x.Status == ReservationStatus.Confirmed), cancellationToken);
        if (occupied) return Result.Failure<ReservationResponse>(Conflict);

        Reservation reservation = new(request.AvailabilityId, requesterUserId, request.DogId, now);
        dbContext.Reservations.Add(reservation);
        dbContext.ReservationHistory.Add(new ReservationHistoryEntry(reservation.Id, requesterUserId, null, ReservationStatus.Pending, "Created", now));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success((await Query().SingleAsync(x => x.Id == reservation.Id, cancellationToken)));
    }

    public async Task<IReadOnlyCollection<ReservationResponse>> ListForTutorAsync(Guid requesterUserId, CancellationToken cancellationToken) =>
        await Query().Where(x => dbContext.Reservations.Any(r => r.Id == x.Id && r.RequesterUserId == requesterUserId)).OrderByDescending(x => x.StartsAtUtc).Take(100).ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyCollection<ReservationResponse>> ListForPartnerAsync(Guid ownerUserId, CancellationToken cancellationToken) =>
        await Query().Where(x => dbContext.Reservations.Any(r => r.Id == x.Id && dbContext.SpaceAvailabilities.Any(a => a.Id == r.AvailabilityId && dbContext.PartnerSpaces.Any(s => s.Id == a.SpaceId && dbContext.PartnerEstablishments.Any(p => p.Id == s.EstablishmentId && p.OwnerUserId == ownerUserId))))).OrderBy(x => x.StartsAtUtc).Take(100).ToArrayAsync(cancellationToken);

    public async Task<Result<ReservationResponse>> ConfirmAsync(Guid ownerUserId, Guid reservationId, CancellationToken cancellationToken)
    {
        Reservation? reservation = await (from r in dbContext.Reservations
                                          join a in dbContext.SpaceAvailabilities on r.AvailabilityId equals a.Id
                                          join s in dbContext.PartnerSpaces on a.SpaceId equals s.Id
                                          join p in dbContext.PartnerEstablishments on s.EstablishmentId equals p.Id
                                          where r.Id == reservationId && p.OwnerUserId == ownerUserId
                                          select r).SingleOrDefaultAsync(cancellationToken);
        if (reservation is null) return Result.Failure<ReservationResponse>(NotFound);
        DateTimeOffset now = timeProvider.GetUtcNow();
        try { reservation.Confirm(now); }
        catch (InvalidOperationException) { return Result.Failure<ReservationResponse>(Conflict); }
        dbContext.ReservationHistory.Add(new ReservationHistoryEntry(reservation.Id, ownerUserId, ReservationStatus.Pending, ReservationStatus.Confirmed, "Confirmed", now));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(await Query().SingleAsync(x => x.Id == reservation.Id, cancellationToken));
    }

    public Task<Result<ReservationResponse>> CancelForTutorAsync(Guid requesterUserId, Guid reservationId, CancellationToken cancellationToken) =>
        CancelAsync(requesterUserId, reservationId, false, cancellationToken);

    public Task<Result<ReservationResponse>> CancelForPartnerAsync(Guid ownerUserId, Guid reservationId, CancellationToken cancellationToken) =>
        CancelAsync(ownerUserId, reservationId, true, cancellationToken);

    public async Task<Result<ReservationResponse>> CompleteAsync(Guid ownerUserId, Guid reservationId, bool paymentReceivedOnSite, CancellationToken cancellationToken)
    {
        Reservation? reservation = await FindForPartnerAsync(ownerUserId, reservationId, cancellationToken);
        if (reservation is null) return Result.Failure<ReservationResponse>(NotFound);
        DateTimeOffset now = timeProvider.GetUtcNow();
        bool started = await dbContext.SpaceAvailabilities.AnyAsync(x => x.Id == reservation.AvailabilityId && x.StartsAtUtc <= now, cancellationToken);
        if (!started) return Result.Failure<ReservationResponse>(Conflict);
        try { reservation.Complete(paymentReceivedOnSite, now); }
        catch (InvalidOperationException) { return Result.Failure<ReservationResponse>(Conflict); }
        dbContext.ReservationHistory.Add(new ReservationHistoryEntry(reservation.Id, ownerUserId, ReservationStatus.Confirmed, ReservationStatus.Completed, paymentReceivedOnSite ? "CompletedPaidOnSite" : "Completed", now));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(await Query().SingleAsync(x => x.Id == reservation.Id, cancellationToken));
    }

    public async Task<Result<ReservationResponse>> MarkNoShowAsync(Guid ownerUserId, Guid reservationId, CancellationToken cancellationToken)
    {
        Reservation? reservation = await FindForPartnerAsync(ownerUserId, reservationId, cancellationToken);
        if (reservation is null) return Result.Failure<ReservationResponse>(NotFound);
        DateTimeOffset now = timeProvider.GetUtcNow();
        bool ended = await dbContext.SpaceAvailabilities.AnyAsync(x => x.Id == reservation.AvailabilityId && x.EndsAtUtc <= now, cancellationToken);
        if (!ended) return Result.Failure<ReservationResponse>(Conflict);
        try { reservation.MarkNoShow(now); }
        catch (InvalidOperationException) { return Result.Failure<ReservationResponse>(Conflict); }
        dbContext.ReservationHistory.Add(new ReservationHistoryEntry(reservation.Id, ownerUserId, ReservationStatus.Confirmed, ReservationStatus.NoShow, "NoShow", now));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(await Query().SingleAsync(x => x.Id == reservation.Id, cancellationToken));
    }

    public async Task<Result<IReadOnlyCollection<ReservationHistoryResponse>>> HistoryAsync(Guid userId, bool isPartner, Guid reservationId, CancellationToken cancellationToken)
    {
        bool allowed = isPartner
            ? await FindForPartnerAsync(userId, reservationId, cancellationToken) is not null
            : await dbContext.Reservations.AnyAsync(x => x.Id == reservationId && x.RequesterUserId == userId, cancellationToken);
        if (!allowed) return Result.Failure<IReadOnlyCollection<ReservationHistoryResponse>>(NotFound);
        ReservationHistoryResponse[] history = await dbContext.ReservationHistory.AsNoTracking().Where(x => x.ReservationId == reservationId)
            .OrderBy(x => x.OccurredAtUtc).Select(x => new ReservationHistoryResponse(x.Id, x.FromStatus.HasValue ? x.FromStatus.Value.ToString() : null, x.ToStatus.ToString(), x.Action, x.OccurredAtUtc)).ToArrayAsync(cancellationToken);
        return Result.Success<IReadOnlyCollection<ReservationHistoryResponse>>(history);
    }

    private async Task<Result<ReservationResponse>> CancelAsync(Guid actorUserId, Guid reservationId, bool asPartner, CancellationToken cancellationToken)
    {
        Reservation? reservation = asPartner
            ? await FindForPartnerAsync(actorUserId, reservationId, cancellationToken)
            : await dbContext.Reservations.SingleOrDefaultAsync(x => x.Id == reservationId && x.RequesterUserId == actorUserId, cancellationToken);
        if (reservation is null) return Result.Failure<ReservationResponse>(NotFound);
        ReservationStatus previous = reservation.Status;
        DateTimeOffset now = timeProvider.GetUtcNow();
        try { reservation.Cancel(actorUserId, now); }
        catch (InvalidOperationException) { return Result.Failure<ReservationResponse>(Conflict); }
        dbContext.ReservationHistory.Add(new ReservationHistoryEntry(reservation.Id, actorUserId, previous, ReservationStatus.Cancelled, asPartner ? "CancelledByPartner" : "CancelledByTutor", now));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(await Query().SingleAsync(x => x.Id == reservation.Id, cancellationToken));
    }

    private Task<Reservation?> FindForPartnerAsync(Guid ownerUserId, Guid reservationId, CancellationToken cancellationToken) =>
        (from r in dbContext.Reservations
         join a in dbContext.SpaceAvailabilities on r.AvailabilityId equals a.Id
         join s in dbContext.PartnerSpaces on a.SpaceId equals s.Id
         join p in dbContext.PartnerEstablishments on s.EstablishmentId equals p.Id
         where r.Id == reservationId && p.OwnerUserId == ownerUserId
         select r).SingleOrDefaultAsync(cancellationToken);

    private IQueryable<ReservationResponse> Query() =>
        from reservation in dbContext.Reservations.AsNoTracking()
        join availability in dbContext.SpaceAvailabilities.AsNoTracking() on reservation.AvailabilityId equals availability.Id
        join space in dbContext.PartnerSpaces.AsNoTracking() on availability.SpaceId equals space.Id
        join dog in dbContext.Dogs.AsNoTracking() on reservation.DogId equals dog.Id
        select new ReservationResponse(reservation.Id, availability.Id, space.Id, space.Name, dog.Id, dog.Name, availability.StartsAtUtc, availability.EndsAtUtc, reservation.Status.ToString(), reservation.PaymentStatus.ToString(), reservation.CreatedAtUtc, reservation.CancelledAtUtc);
}
