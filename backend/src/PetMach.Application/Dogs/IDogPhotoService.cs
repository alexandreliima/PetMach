using PetMach.Contracts.Dogs;
using PetMach.Domain.SharedKernel;

namespace PetMach.Application.Dogs;

public interface IDogPhotoService
{
    Task<Result<DogPhotoResponse>> AddAsync(Guid userId, Guid dogId, Stream content, string contentType, long length, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DogPhotoResponse>> ListAsync(Guid userId, Guid dogId, CancellationToken cancellationToken);
}
