using PetMach.Contracts.Adoption;
using PetMach.Domain.SharedKernel;

namespace PetMach.Application.Adoption;

public interface IAdoptionService
{
    Task<Result<AdoptionProfileResponse>> CreateAsync(Guid userId, CreateAdoptionProfileRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AdoptionProfileResponse>> ListAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result> SuspendAsync(Guid userId, Guid profileId, CancellationToken cancellationToken);
    Task<Result<AdoptionApplicationResponse>> ApplyAsync(Guid userId, Guid profileId, CreateAdoptionApplicationRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AdoptionApplicationResponse>> ListMyApplicationsAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result<IReadOnlyCollection<AdoptionApplicationResponse>>> ListProfileApplicationsAsync(Guid ownerUserId, Guid profileId, CancellationToken cancellationToken);
    Task<Result<AdoptionApplicationResponse>> TransitionApplicationAsync(Guid actorUserId, Guid applicationId, string transition, CancellationToken cancellationToken);
    Task<Result<IReadOnlyCollection<AdoptionApplicationHistoryResponse>>> ApplicationHistoryAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken);
}
