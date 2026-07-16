using PetMach.Contracts.Matches;
using PetMach.Domain.SharedKernel;

namespace PetMach.Application.Matches;

public interface IMatchService
{
    Task<IReadOnlyCollection<MatchResponse>> ListAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result> EndAsync(Guid userId, Guid matchId, CancellationToken cancellationToken);
}
