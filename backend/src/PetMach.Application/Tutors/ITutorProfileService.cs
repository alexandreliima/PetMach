using PetMach.Contracts.Tutors;
using PetMach.Domain.SharedKernel;

namespace PetMach.Application.Tutors;

public interface ITutorProfileService
{
    Task<Result<TutorProfileResponse>> GetAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result<TutorProfileResponse>> UpsertAsync(Guid userId, UpsertTutorProfileRequest request, CancellationToken cancellationToken);
}
