using PetMach.Contracts.Partners;
using PetMach.Domain.SharedKernel;

namespace PetMach.Application.Partners;

public interface IPartnerService
{
    Task<Result<PartnerManagementResponse>> CreateAsync(Guid ownerUserId, CreatePartnerRequest request, CancellationToken cancellationToken);
    Task<Result<PartnerManagementResponse>> GetManagedAsync(Guid ownerUserId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PartnerSpaceResponse>> ListManagedSpacesAsync(Guid ownerUserId, CancellationToken cancellationToken);
    Task<Result<PartnerSpaceResponse>> CreateSpaceAsync(Guid ownerUserId, Guid establishmentId, CreateSpaceRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PartnerSpaceResponse>> ListSpacesAsync(string? city, string? state, CancellationToken cancellationToken);
    Task<Result<SpaceAvailabilityResponse>> CreateAvailabilityAsync(Guid ownerUserId, Guid spaceId, CreateAvailabilityRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SpaceAvailabilityResponse>> ListAvailabilityAsync(Guid spaceId, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, CancellationToken cancellationToken);
}
