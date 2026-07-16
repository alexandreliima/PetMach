using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PetMach.Application.Tutors;
using PetMach.Contracts.Tutors;
using PetMach.Domain.SharedKernel;
using PetMach.Domain.Tutors;
using PetMach.Infrastructure.Persistence;

namespace PetMach.Infrastructure.Tutors;

internal sealed class TutorProfileService(
    PetMachDbContext dbContext,
    IValidator<UpsertTutorProfileRequest> validator,
    TimeProvider timeProvider) : ITutorProfileService
{
    public async Task<Result<TutorProfileResponse>> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        TutorProfile? profile = await dbContext.TutorProfiles.AsNoTracking().SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        return profile is null
            ? Result.Failure<TutorProfileResponse>(TutorErrors.NotFound)
            : Result.Success(ToResponse(profile));
    }

    public async Task<Result<TutorProfileResponse>> UpsertAsync(Guid userId, UpsertTutorProfileRequest request, CancellationToken cancellationToken)
    {
        if (!(await validator.ValidateAsync(request, cancellationToken)).IsValid)
            return Result.Failure<TutorProfileResponse>(TutorErrors.InvalidProfile);

        TutorProfile? profile = await dbContext.TutorProfiles.SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        DateTimeOffset now = timeProvider.GetUtcNow();
        if (profile is null)
        {
            Result<TutorProfile> created = TutorProfile.Create(userId, request.FirstName, request.LastName, request.Phone, request.City, request.State, request.Biography, request.ShowCity, request.AllowDiscovery, now);
            if (created.IsFailure) return Result.Failure<TutorProfileResponse>(created.Error);
            profile = created.Value;
            dbContext.TutorProfiles.Add(profile);
        }
        else
        {
            Result updated = profile.Update(request.FirstName, request.LastName, request.Phone, request.City, request.State, request.Biography, request.ShowCity, request.AllowDiscovery, now);
            if (updated.IsFailure) return Result.Failure<TutorProfileResponse>(updated.Error);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(ToResponse(profile));
    }

    private static TutorProfileResponse ToResponse(TutorProfile profile) => new(
        profile.Id, profile.FirstName, profile.LastName, profile.Phone, profile.City, profile.State,
        profile.Biography, profile.ShowCity, profile.AllowDiscovery, profile.CreatedAtUtc, profile.UpdatedAtUtc);
}
