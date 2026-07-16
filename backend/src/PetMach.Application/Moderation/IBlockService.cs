using PetMach.Domain.SharedKernel;

namespace PetMach.Application.Moderation;

public interface IBlockService
{
    Task<Result> BlockDogOwnerAsync(Guid userId, Guid targetDogId, CancellationToken cancellationToken);
}
