using Microsoft.EntityFrameworkCore;
using PetMach.Application.Moderation;
using PetMach.Domain.Discovery;
using PetMach.Domain.Moderation;
using PetMach.Domain.SharedKernel;
using PetMach.Infrastructure.Persistence;

namespace PetMach.Infrastructure.Moderation;

internal sealed class BlockService(PetMachDbContext dbContext, TimeProvider timeProvider) : IBlockService
{
    public async Task<Result> BlockDogOwnerAsync(Guid userId, Guid targetDogId, CancellationToken cancellationToken)
    {
        Guid? targetOwnerId = await dbContext.Dogs.Where(x => x.Id == targetDogId).Select(x => (Guid?)x.OwnerUserId).SingleOrDefaultAsync(cancellationToken);
        if (targetOwnerId is null || targetOwnerId == userId) return Result.Failure(DiscoveryErrors.Invalid);
        if (await dbContext.BlockedUsers.AnyAsync(x => x.UserId == userId && x.BlockedUserId == targetOwnerId, cancellationToken)) return Result.Success();

        dbContext.BlockedUsers.Add(new BlockedUser(userId, targetOwnerId.Value, timeProvider.GetUtcNow()));
        Guid[] myDogs = await dbContext.Dogs.Where(x => x.OwnerUserId == userId).Select(x => x.Id).ToArrayAsync(cancellationToken);
        Guid[] otherDogs = await dbContext.Dogs.Where(x => x.OwnerUserId == targetOwnerId).Select(x => x.Id).ToArrayAsync(cancellationToken);
        var matches = await dbContext.DogMatches.Where(x => x.EndedAtUtc == null &&
            (myDogs.Contains(x.DogAId) && otherDogs.Contains(x.DogBId) || myDogs.Contains(x.DogBId) && otherDogs.Contains(x.DogAId)))
            .ToArrayAsync(cancellationToken);
        foreach (var match in matches) match.End(timeProvider.GetUtcNow());
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
