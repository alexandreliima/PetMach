using PetMach.Contracts.Discovery;
using PetMach.Domain.SharedKernel;

namespace PetMach.Application.Discovery;

public interface IDiscoveryService
{
    Task<Result<DiscoveryPageResponse>> DiscoverAsync(Guid userId, DiscoveryFilterRequest request, CancellationToken cancellationToken);
    Task<Result<LikeDogResponse>> LikeAsync(Guid userId, Guid targetDogId, LikeDogRequest request, CancellationToken cancellationToken);
    Task<Result> PassAsync(Guid userId, Guid targetDogId, PassDogRequest request, CancellationToken cancellationToken);
    Task<Result<DiscoveryImage>> GetPrimaryPhotoAsync(Guid userId, Guid dogId, CancellationToken cancellationToken);
}

public sealed record DiscoveryImage(byte[] Content, string ContentType);
