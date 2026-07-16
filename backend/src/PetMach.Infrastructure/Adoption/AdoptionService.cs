using Microsoft.EntityFrameworkCore;
using PetMach.Application.Adoption;
using PetMach.Contracts.Adoption;
using PetMach.Domain.Adoption;
using PetMach.Domain.Dogs;
using PetMach.Domain.SharedKernel;
using PetMach.Infrastructure.Persistence;

namespace PetMach.Infrastructure.Adoption;

internal sealed class AdoptionService(PetMachDbContext dbContext, TimeProvider timeProvider) : IAdoptionService
{
    private const string TermsVersion = "2026-07-16";
    private static readonly DomainError Invalid = new("adoption.invalid", "Os dados da publicação de adoção são inválidos.");
    private static readonly DomainError Conflict = new("adoption.conflict", "Este cão já possui uma publicação de adoção.");
    private static readonly DomainError NotFound = new("adoption.not_found", "Publicação de adoção não encontrada.");
    private static readonly DomainError ApplicationNotFound = new("adoption.application_not_found", "Candidatura de adoção não encontrada.");
    private static readonly DomainError ApplicationConflict = new("adoption.application_conflict", "A candidatura já existe ou não permite esta transição.");

    public async Task<Result<AdoptionProfileResponse>> CreateAsync(Guid userId, CreateAdoptionProfileRequest request, CancellationToken cancellationToken)
    {
        if (!request.TermsAccepted) return Result.Failure<AdoptionProfileResponse>(Invalid);
        Dog? dog = await dbContext.Dogs.SingleOrDefaultAsync(x => x.Id == request.DogId && x.OwnerUserId == userId && x.Status == DogProfileStatus.Active, cancellationToken);
        if (dog is null) return Result.Failure<AdoptionProfileResponse>(Invalid);
        if (await dbContext.AdoptionProfiles.AnyAsync(x => x.DogId == dog.Id, cancellationToken)) return Result.Failure<AdoptionProfileResponse>(Conflict);
        AdoptionProfile profile;
        try { profile = new AdoptionProfile(dog.Id, userId, request.Story, request.Requirements, TermsVersion, timeProvider.GetUtcNow()); }
        catch (ArgumentException) { return Result.Failure<AdoptionProfileResponse>(Invalid); }
        dbContext.AdoptionProfiles.Add(profile);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(await Query(userId).SingleAsync(x => x.Id == profile.Id, cancellationToken));
    }

    public async Task<IReadOnlyCollection<AdoptionProfileResponse>> ListAsync(Guid userId, CancellationToken cancellationToken) =>
        await Query(userId).Where(x => x.Status == AdoptionStatus.Available.ToString() || x.IsMine).OrderByDescending(x => x.CreatedAtUtc).Take(100).ToArrayAsync(cancellationToken);

    public async Task<Result> SuspendAsync(Guid userId, Guid profileId, CancellationToken cancellationToken)
    {
        AdoptionProfile? profile = await dbContext.AdoptionProfiles.SingleOrDefaultAsync(x => x.Id == profileId && x.PublisherUserId == userId, cancellationToken);
        if (profile is null) return Result.Failure(NotFound);
        try { profile.Suspend(timeProvider.GetUtcNow()); }
        catch (InvalidOperationException) { return Result.Failure(Conflict); }
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<AdoptionApplicationResponse>> ApplyAsync(Guid userId, Guid profileId, CreateAdoptionApplicationRequest request, CancellationToken cancellationToken)
    {
        if (!request.TermsAccepted) return Result.Failure<AdoptionApplicationResponse>(Invalid);
        AdoptionProfile? profile = await dbContext.AdoptionProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.Id == profileId && x.Status == AdoptionStatus.Available, cancellationToken);
        if (profile is null) return Result.Failure<AdoptionApplicationResponse>(NotFound);
        if (profile.PublisherUserId == userId || await IsBlockedAsync(userId, profile.PublisherUserId, cancellationToken)) return Result.Failure<AdoptionApplicationResponse>(Invalid);
        if (await dbContext.AdoptionApplications.AnyAsync(x => x.ProfileId == profileId && x.ApplicantUserId == userId, cancellationToken)) return Result.Failure<AdoptionApplicationResponse>(ApplicationConflict);
        AdoptionApplication application;
        DateTimeOffset now = timeProvider.GetUtcNow();
        try { application = new AdoptionApplication(profileId, userId, request.Motivation, request.Experience, request.HousingContext, TermsVersion, now); }
        catch (ArgumentException) { return Result.Failure<AdoptionApplicationResponse>(Invalid); }
        dbContext.AdoptionApplications.Add(application);
        dbContext.AdoptionApplicationHistory.Add(new AdoptionApplicationHistory(application.Id, userId, null, AdoptionApplicationStatus.Submitted, now));
        try { await dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException) { return Result.Failure<AdoptionApplicationResponse>(ApplicationConflict); }
        return Result.Success(await ApplicationQuery(userId).SingleAsync(x => x.Id == application.Id, cancellationToken));
    }

    public async Task<IReadOnlyCollection<AdoptionApplicationResponse>> ListMyApplicationsAsync(Guid userId, CancellationToken cancellationToken) =>
        await ApplicationQuery(userId).Where(x => x.IsMine).OrderByDescending(x => x.CreatedAtUtc).Take(100).ToArrayAsync(cancellationToken);

    public async Task<Result<IReadOnlyCollection<AdoptionApplicationResponse>>> ListProfileApplicationsAsync(Guid ownerUserId, Guid profileId, CancellationToken cancellationToken)
    {
        if (!await dbContext.AdoptionProfiles.AnyAsync(x => x.Id == profileId && x.PublisherUserId == ownerUserId, cancellationToken))
            return Result.Failure<IReadOnlyCollection<AdoptionApplicationResponse>>(NotFound);
        AdoptionApplicationResponse[] items = await ApplicationQuery(ownerUserId).Where(x => x.ProfileId == profileId).OrderByDescending(x => x.CreatedAtUtc).Take(100).ToArrayAsync(cancellationToken);
        return Result.Success<IReadOnlyCollection<AdoptionApplicationResponse>>(items);
    }

    public async Task<Result<AdoptionApplicationResponse>> TransitionApplicationAsync(Guid actorUserId, Guid applicationId, string transition, CancellationToken cancellationToken)
    {
        AdoptionApplication? application = await dbContext.AdoptionApplications.SingleOrDefaultAsync(x => x.Id == applicationId, cancellationToken);
        if (application is null) return Result.Failure<AdoptionApplicationResponse>(ApplicationNotFound);
        AdoptionProfile? profile = await dbContext.AdoptionProfiles.SingleOrDefaultAsync(x => x.Id == application.ProfileId, cancellationToken);
        if (profile is null) return Result.Failure<AdoptionApplicationResponse>(NotFound);
        bool applicantAction = transition == "withdraw" && application.ApplicantUserId == actorUserId;
        bool ownerAction = transition is "review" or "approve" or "reject" && profile.PublisherUserId == actorUserId;
        if (!applicantAction && !ownerAction) return Result.Failure<AdoptionApplicationResponse>(ApplicationNotFound);
        AdoptionApplicationStatus previous = application.Status;
        DateTimeOffset now = timeProvider.GetUtcNow();
        try
        {
            switch (transition)
            {
                case "review": application.StartReview(now); break;
                case "approve": application.Approve(now); profile.MarkInProgress(now); break;
                case "reject": application.Reject(now); break;
                case "withdraw": application.Withdraw(now); break;
                default: return Result.Failure<AdoptionApplicationResponse>(Invalid);
            }
        }
        catch (InvalidOperationException) { return Result.Failure<AdoptionApplicationResponse>(ApplicationConflict); }
        dbContext.AdoptionApplicationHistory.Add(new AdoptionApplicationHistory(application.Id, actorUserId, previous, application.Status, now));
        try { await dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException) { return Result.Failure<AdoptionApplicationResponse>(ApplicationConflict); }
        return Result.Success(await ApplicationQuery(actorUserId).SingleAsync(x => x.Id == application.Id, cancellationToken));
    }

    public async Task<Result<IReadOnlyCollection<AdoptionApplicationHistoryResponse>>> ApplicationHistoryAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken)
    {
        bool allowed = await dbContext.AdoptionApplications.AnyAsync(x => x.Id == applicationId && (x.ApplicantUserId == userId || dbContext.AdoptionProfiles.Any(p => p.Id == x.ProfileId && p.PublisherUserId == userId)), cancellationToken);
        if (!allowed) return Result.Failure<IReadOnlyCollection<AdoptionApplicationHistoryResponse>>(ApplicationNotFound);
        AdoptionApplicationHistoryResponse[] history = await dbContext.AdoptionApplicationHistory.AsNoTracking().Where(x => x.ApplicationId == applicationId).OrderBy(x => x.OccurredAtUtc)
            .Select(x => new AdoptionApplicationHistoryResponse(x.FromStatus.HasValue ? x.FromStatus.Value.ToString() : null, x.ToStatus.ToString(), x.OccurredAtUtc)).ToArrayAsync(cancellationToken);
        return Result.Success<IReadOnlyCollection<AdoptionApplicationHistoryResponse>>(history);
    }

    private IQueryable<AdoptionProfileResponse> Query(Guid userId) =>
        from profile in dbContext.AdoptionProfiles.AsNoTracking()
        join dog in dbContext.Dogs.AsNoTracking() on profile.DogId equals dog.Id
        join tutor in dbContext.TutorProfiles.AsNoTracking() on profile.PublisherUserId equals tutor.UserId
        where profile.PublisherUserId == userId || !dbContext.BlockedUsers.Any(x => x.UserId == userId && x.BlockedUserId == profile.PublisherUserId || x.UserId == profile.PublisherUserId && x.BlockedUserId == userId)
        select new AdoptionProfileResponse(profile.Id, dog.Id, dog.Name, dog.Breed, dog.Size.ToString(), tutor.ShowCity ? tutor.City + "/" + tutor.State : tutor.State, profile.Story, profile.Requirements, profile.Status.ToString(), profile.CreatedAtUtc, profile.PublisherUserId == userId);

    private IQueryable<AdoptionApplicationResponse> ApplicationQuery(Guid userId) =>
        from application in dbContext.AdoptionApplications.AsNoTracking()
        join profile in dbContext.AdoptionProfiles.AsNoTracking() on application.ProfileId equals profile.Id
        join dog in dbContext.Dogs.AsNoTracking() on profile.DogId equals dog.Id
        join applicant in dbContext.TutorProfiles.AsNoTracking() on application.ApplicantUserId equals applicant.UserId
        where application.ApplicantUserId == userId || profile.PublisherUserId == userId
        select new AdoptionApplicationResponse(application.Id, profile.Id, dog.Name, applicant.FirstName, application.Motivation, application.Experience, application.HousingContext, application.Status.ToString(), application.CreatedAtUtc, application.ApplicantUserId == userId);

    private Task<bool> IsBlockedAsync(Guid userId, Guid publisherUserId, CancellationToken cancellationToken) =>
        dbContext.BlockedUsers.AnyAsync(x => x.UserId == userId && x.BlockedUserId == publisherUserId || x.UserId == publisherUserId && x.BlockedUserId == userId, cancellationToken);
}
