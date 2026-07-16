using PetMach.Contracts.Dogs;
using PetMach.Domain.SharedKernel;

namespace PetMach.Application.Dogs;

public interface IDogService
{
    Task<IReadOnlyCollection<DogResponse>> ListAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result<DogResponse>> GetAsync(Guid userId, Guid dogId, CancellationToken cancellationToken);
    Task<Result<DogResponse>> CreateAsync(Guid userId, UpsertDogRequest request, CancellationToken cancellationToken);
    Task<Result<DogResponse>> UpdateAsync(Guid userId, Guid dogId, UpsertDogRequest request, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(Guid userId, Guid dogId, CancellationToken cancellationToken);
}
