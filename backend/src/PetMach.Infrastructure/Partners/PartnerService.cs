using Microsoft.EntityFrameworkCore;
using PetMach.Application.Partners;
using PetMach.Contracts.Partners;
using PetMach.Domain.Partners;
using PetMach.Domain.SharedKernel;
using PetMach.Infrastructure.Persistence;

namespace PetMach.Infrastructure.Partners;

internal sealed class PartnerService(PetMachDbContext dbContext, TimeProvider timeProvider) : IPartnerService
{
    private static readonly DomainError Invalid = new("partners.invalid", "Os dados do parceiro ou espaço são inválidos.");
    private static readonly DomainError Conflict = new("partners.conflict", "Já existe um parceiro para estes dados.");
    private static readonly DomainError NotFound = new("partners.not_found", "Parceiro não encontrado.");
    private static readonly DomainError AvailabilityConflict = new("partners.availability_conflict", "A janela de disponibilidade se sobrepõe a outra janela ativa.");

    public async Task<Result<PartnerManagementResponse>> CreateAsync(Guid ownerUserId, CreatePartnerRequest request, CancellationToken cancellationToken)
    {
        if (!Valid(request) || !ValidTimeZone(request.TimeZoneId)) return Result.Failure<PartnerManagementResponse>(Invalid);
        string registration = request.RegistrationNumber.Trim();
        if (await dbContext.PartnerEstablishments.AnyAsync(x => x.OwnerUserId == ownerUserId || x.RegistrationNumber == registration, cancellationToken))
            return Result.Failure<PartnerManagementResponse>(Conflict);
        PartnerEstablishment partner = new(ownerUserId, request.LegalName, request.DisplayName, registration, request.City, request.State, request.TimeZoneId, timeProvider.GetUtcNow());
        dbContext.PartnerEstablishments.Add(partner);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(new PartnerManagementResponse(partner.Id, partner.LegalName, partner.DisplayName, partner.RegistrationNumber, partner.City, partner.State, partner.TimeZoneId, partner.IsActive));
    }

    public async Task<Result<PartnerManagementResponse>> GetManagedAsync(Guid ownerUserId, CancellationToken cancellationToken)
    {
        PartnerEstablishment? partner = await dbContext.PartnerEstablishments.AsNoTracking().SingleOrDefaultAsync(x => x.OwnerUserId == ownerUserId && x.IsActive, cancellationToken);
        return partner is null ? Result.Failure<PartnerManagementResponse>(NotFound) : Result.Success(ToManagementResponse(partner));
    }

    public async Task<IReadOnlyCollection<PartnerSpaceResponse>> ListManagedSpacesAsync(Guid ownerUserId, CancellationToken cancellationToken) =>
        await (from space in dbContext.PartnerSpaces.AsNoTracking()
               join partner in dbContext.PartnerEstablishments.AsNoTracking() on space.EstablishmentId equals partner.Id
               where partner.OwnerUserId == ownerUserId && partner.IsActive && space.IsActive
               orderby space.Name
               select new PartnerSpaceResponse(space.Id, partner.Id, partner.DisplayName, space.Name, space.Description, space.Capacity, space.InformationalPrice, partner.City, partner.State, partner.TimeZoneId))
            .Take(100).ToArrayAsync(cancellationToken);

    public async Task<Result<PartnerSpaceResponse>> CreateSpaceAsync(Guid ownerUserId, Guid establishmentId, CreateSpaceRequest request, CancellationToken cancellationToken)
    {
        PartnerEstablishment? partner = await dbContext.PartnerEstablishments.SingleOrDefaultAsync(x => x.Id == establishmentId && x.OwnerUserId == ownerUserId && x.IsActive, cancellationToken);
        if (partner is null) return Result.Failure<PartnerSpaceResponse>(NotFound);
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Description) || request.Name.Trim().Length > 120 || request.Description.Trim().Length > 1000 || request.Capacity is < 1 or > 1000 || request.InformationalPrice < 0)
            return Result.Failure<PartnerSpaceResponse>(Invalid);
        PartnerSpace space = new(partner.Id, request.Name, request.Description, request.Capacity, request.InformationalPrice, timeProvider.GetUtcNow());
        dbContext.PartnerSpaces.Add(space);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(ToResponse(space, partner));
    }

    public async Task<IReadOnlyCollection<PartnerSpaceResponse>> ListSpacesAsync(string? city, string? state, CancellationToken cancellationToken)
    {
        string? normalizedState = string.IsNullOrWhiteSpace(state) ? null : state.Trim().ToUpperInvariant();
        IQueryable<PartnerSpaceResponse> query =
            from space in dbContext.PartnerSpaces.AsNoTracking()
            join partner in dbContext.PartnerEstablishments.AsNoTracking() on space.EstablishmentId equals partner.Id
            where space.IsActive && partner.IsActive
            orderby partner.DisplayName, space.Name
            select new PartnerSpaceResponse(space.Id, partner.Id, partner.DisplayName, space.Name, space.Description, space.Capacity, space.InformationalPrice, partner.City, partner.State, partner.TimeZoneId);
        if (!string.IsNullOrWhiteSpace(city)) query = query.Where(x => x.City == city.Trim());
        if (normalizedState is not null) query = query.Where(x => x.State == normalizedState);
        return await query.Take(100).ToArrayAsync(cancellationToken);
    }

    public async Task<Result<SpaceAvailabilityResponse>> CreateAvailabilityAsync(Guid ownerUserId, Guid spaceId, CreateAvailabilityRequest request, CancellationToken cancellationToken)
    {
        bool ownsSpace = await (
            from space in dbContext.PartnerSpaces
            join partner in dbContext.PartnerEstablishments on space.EstablishmentId equals partner.Id
            where space.Id == spaceId && space.IsActive && partner.IsActive && partner.OwnerUserId == ownerUserId
            select space.Id).AnyAsync(cancellationToken);
        if (!ownsSpace) return Result.Failure<SpaceAvailabilityResponse>(NotFound);

        SpaceAvailability availability;
        try
        {
            availability = new SpaceAvailability(spaceId, request.StartsAtUtc, request.EndsAtUtc, timeProvider.GetUtcNow());
        }
        catch (ArgumentException)
        {
            return Result.Failure<SpaceAvailabilityResponse>(Invalid);
        }

        bool overlaps = await dbContext.SpaceAvailabilities.AnyAsync(
            x => x.SpaceId == spaceId && x.IsAvailable && x.StartsAtUtc < availability.EndsAtUtc && availability.StartsAtUtc < x.EndsAtUtc,
            cancellationToken);
        if (overlaps) return Result.Failure<SpaceAvailabilityResponse>(AvailabilityConflict);

        dbContext.SpaceAvailabilities.Add(availability);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(ToResponse(availability));
    }

    public async Task<IReadOnlyCollection<SpaceAvailabilityResponse>> ListAvailabilityAsync(Guid spaceId, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, CancellationToken cancellationToken)
    {
        DateTimeOffset rangeStart = (fromUtc ?? timeProvider.GetUtcNow()).ToUniversalTime();
        DateTimeOffset rangeEnd = (toUtc ?? rangeStart.AddDays(30)).ToUniversalTime();
        if (rangeEnd <= rangeStart || rangeEnd - rangeStart > TimeSpan.FromDays(90)) return [];

        return await (
            from availability in dbContext.SpaceAvailabilities.AsNoTracking()
            join space in dbContext.PartnerSpaces.AsNoTracking() on availability.SpaceId equals space.Id
            join partner in dbContext.PartnerEstablishments.AsNoTracking() on space.EstablishmentId equals partner.Id
            where availability.SpaceId == spaceId && availability.IsAvailable && space.IsActive && partner.IsActive && availability.EndsAtUtc > rangeStart && availability.StartsAtUtc < rangeEnd
            orderby availability.StartsAtUtc
            select new SpaceAvailabilityResponse(availability.Id, availability.SpaceId, availability.StartsAtUtc, availability.EndsAtUtc, availability.IsAvailable))
            .Take(100)
            .ToArrayAsync(cancellationToken);
    }

    private static bool Valid(CreatePartnerRequest request) =>
        !string.IsNullOrWhiteSpace(request.LegalName) && !string.IsNullOrWhiteSpace(request.DisplayName) && !string.IsNullOrWhiteSpace(request.RegistrationNumber) &&
        !string.IsNullOrWhiteSpace(request.City) && !string.IsNullOrWhiteSpace(request.State) && !string.IsNullOrWhiteSpace(request.TimeZoneId) &&
        request.LegalName.Trim().Length <= 180 && request.DisplayName.Trim().Length <= 120 && request.RegistrationNumber.Trim().Length <= 32 &&
        request.City.Trim().Length <= 120 && request.State.Trim().Length <= 50 && request.TimeZoneId.Trim().Length <= 100;

    private static bool ValidTimeZone(string id)
    {
        try { _ = TimeZoneInfo.FindSystemTimeZoneById(id.Trim()); return true; }
        catch (TimeZoneNotFoundException) { return false; }
        catch (InvalidTimeZoneException) { return false; }
    }

    private static PartnerSpaceResponse ToResponse(PartnerSpace space, PartnerEstablishment partner) =>
        new(space.Id, partner.Id, partner.DisplayName, space.Name, space.Description, space.Capacity, space.InformationalPrice, partner.City, partner.State, partner.TimeZoneId);

    private static PartnerManagementResponse ToManagementResponse(PartnerEstablishment partner) =>
        new(partner.Id, partner.LegalName, partner.DisplayName, partner.RegistrationNumber, partner.City, partner.State, partner.TimeZoneId, partner.IsActive);

    private static SpaceAvailabilityResponse ToResponse(SpaceAvailability availability) =>
        new(availability.Id, availability.SpaceId, availability.StartsAtUtc, availability.EndsAtUtc, availability.IsAvailable);
}
