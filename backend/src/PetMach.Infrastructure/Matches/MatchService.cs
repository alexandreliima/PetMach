using Microsoft.EntityFrameworkCore;
using PetMach.Application.Matches;
using PetMach.Contracts.Matches;
using PetMach.Domain.Discovery;
using PetMach.Domain.Matches;
using PetMach.Domain.SharedKernel;
using PetMach.Infrastructure.Persistence;

namespace PetMach.Infrastructure.Matches;

internal sealed class MatchService(PetMachDbContext dbContext, TimeProvider timeProvider) : IMatchService
{
    public async Task<IReadOnlyCollection<MatchResponse>> ListAsync(Guid userId, CancellationToken cancellationToken)
    {
        MatchResponse[] firstSide = await (
            from match in dbContext.DogMatches.AsNoTracking()
            join mine in dbContext.Dogs.AsNoTracking() on match.DogAId equals mine.Id
            join other in dbContext.Dogs.AsNoTracking() on match.DogBId equals other.Id
            where match.EndedAtUtc == null && mine.OwnerUserId == userId
            select new MatchResponse(match.Id, mine.Id, other.Id, other.Name, other.Breed, match.CreatedAtUtc))
            .ToArrayAsync(cancellationToken);

        MatchResponse[] secondSide = await (
            from match in dbContext.DogMatches.AsNoTracking()
            join mine in dbContext.Dogs.AsNoTracking() on match.DogBId equals mine.Id
            join other in dbContext.Dogs.AsNoTracking() on match.DogAId equals other.Id
            where match.EndedAtUtc == null && mine.OwnerUserId == userId
            select new MatchResponse(match.Id, mine.Id, other.Id, other.Name, other.Breed, match.CreatedAtUtc))
            .ToArrayAsync(cancellationToken);

        return firstSide.Concat(secondSide).OrderByDescending(x => x.CreatedAtUtc).ToArray();
    }

    public async Task<Result> EndAsync(Guid userId, Guid matchId, CancellationToken cancellationToken)
    {
        DogMatch? match = await dbContext.DogMatches.SingleOrDefaultAsync(x => x.Id == matchId && x.EndedAtUtc == null, cancellationToken);
        if (match is null) return Result.Failure(DiscoveryErrors.MatchNotFound);
        bool participant = await dbContext.Dogs.AnyAsync(x => (x.Id == match.DogAId || x.Id == match.DogBId) && x.OwnerUserId == userId, cancellationToken);
        if (!participant) return Result.Failure(DiscoveryErrors.MatchNotFound);
        match.End(timeProvider.GetUtcNow());
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
