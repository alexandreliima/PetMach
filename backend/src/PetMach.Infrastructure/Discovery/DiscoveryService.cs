using System.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using PetMach.Application.Discovery;
using PetMach.Contracts.Discovery;
using PetMach.Contracts.Dogs;
using PetMach.Domain.Chat;
using PetMach.Domain.Discovery;
using PetMach.Domain.Dogs;
using PetMach.Domain.Matches;
using PetMach.Domain.Notifications;
using PetMach.Domain.SharedKernel;
using PetMach.Infrastructure.Persistence;

namespace PetMach.Infrastructure.Discovery;

internal sealed class DiscoveryService(PetMachDbContext dbContext, IWebHostEnvironment environment, TimeProvider timeProvider) : IDiscoveryService
{
    public async Task<Result<DiscoveryPageResponse>> DiscoverAsync(Guid userId, DiscoveryFilterRequest request, CancellationToken cancellationToken)
    {
        if (request.SourceDogId == Guid.Empty || request.Page < 1 || request.PageSize is < 1 or > 50)
            return Result.Failure<DiscoveryPageResponse>(DiscoveryErrors.Invalid);

        bool sourceOwned = await dbContext.Dogs.AnyAsync(x => x.Id == request.SourceDogId && x.OwnerUserId == userId && x.Status == DogProfileStatus.Active, cancellationToken);
        if (!sourceOwned) return Result.Failure<DiscoveryPageResponse>(DiscoveryErrors.DogNotFound);

        DateOnly today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        IQueryable<Dog> candidates = dbContext.Dogs.AsNoTracking().Where(x =>
            x.OwnerUserId != userId &&
            x.Status == DogProfileStatus.Active &&
            dbContext.TutorProfiles.Any(t => t.UserId == x.OwnerUserId && t.AllowDiscovery) &&
            !dbContext.BlockedUsers.Any(b => (b.UserId == userId && b.BlockedUserId == x.OwnerUserId) || (b.UserId == x.OwnerUserId && b.BlockedUserId == userId)) &&
            !dbContext.DogLikes.Any(l => l.SourceDogId == request.SourceDogId && l.TargetDogId == x.Id) &&
            !dbContext.DogPasses.Any(p => p.SourceDogId == request.SourceDogId && p.TargetDogId == x.Id));

        if (request.Sex.HasValue) candidates = candidates.Where(x => x.Sex == (DogSex)request.Sex.Value);
        if (request.Size.HasValue) candidates = candidates.Where(x => x.Size == (DogSize)request.Size.Value);
        if (!string.IsNullOrWhiteSpace(request.Breed)) candidates = candidates.Where(x => x.Breed == request.Breed.Trim());
        if (request.EnergyLevel.HasValue) candidates = candidates.Where(x => x.EnergyLevel == (EnergyLevel)request.EnergyLevel.Value);
        if (request.Goal.HasValue) candidates = candidates.Where(x => x.Goal == (DogGoal)request.Goal.Value);
        if (request.Neutered.HasValue) candidates = candidates.Where(x => x.Neutered == request.Neutered.Value);
        if (request.VaccinationUpToDate.HasValue)
        {
            candidates = request.VaccinationUpToDate.Value
                ? candidates.Where(x => dbContext.DogVaccinations.Any(v => v.DogId == x.Id) && !dbContext.DogVaccinations.Any(v => v.DogId == x.Id && v.NextDoseOn != null && v.NextDoseOn < today))
                : candidates.Where(x => !dbContext.DogVaccinations.Any(v => v.DogId == x.Id) || dbContext.DogVaccinations.Any(v => v.DogId == x.Id && v.NextDoseOn != null && v.NextDoseOn < today));
        }

        DiscoveryDogResponse[] page = await candidates
            .OrderBy(x => x.CreatedAtUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize + 1)
            .Select(x => new DiscoveryDogResponse(
                x.Id,
                x.Name,
                x.BirthDate,
                x.ApproximateAge,
                (DogSexContract)x.Sex,
                x.Breed,
                (DogSizeContract)x.Size,
                x.Temperament,
                (EnergyLevelContract)x.EnergyLevel,
                (DogGoalContract)x.Goal,
                x.Neutered,
                dbContext.DogVaccinations.Any(v => v.DogId == x.Id) && !dbContext.DogVaccinations.Any(v => v.DogId == x.Id && v.NextDoseOn != null && v.NextDoseOn < today),
                dbContext.TutorProfiles.Where(t => t.UserId == x.OwnerUserId).Select(t => t.ShowCity ? t.City + " - " + t.State : t.State).First(),
                dbContext.DogPhotos.Any(p => p.DogId == x.Id && p.IsPrimary) ? $"/api/v1/discovery/dogs/{x.Id}/photo" : null))
            .ToArrayAsync(cancellationToken);

        bool hasMore = page.Length > request.PageSize;
        return Result.Success(new DiscoveryPageResponse(page.Take(request.PageSize).ToArray(), request.Page, request.PageSize, hasMore));
    }

    public async Task<Result<LikeDogResponse>> LikeAsync(Guid userId, Guid targetDogId, LikeDogRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        Dog? source = await dbContext.Dogs.SingleOrDefaultAsync(x => x.Id == request.SourceDogId && x.OwnerUserId == userId && x.Status == DogProfileStatus.Active, cancellationToken);
        Dog? target = await dbContext.Dogs.SingleOrDefaultAsync(x => x.Id == targetDogId && x.OwnerUserId != userId && x.Status == DogProfileStatus.Active && dbContext.TutorProfiles.Any(t => t.UserId == x.OwnerUserId && t.AllowDiscovery), cancellationToken);
        if (source is null || target is null)
            return Result.Failure<LikeDogResponse>(DiscoveryErrors.DogNotFound);
        if (await IsBlockedAsync(userId, target.OwnerUserId, cancellationToken))
            return Result.Failure<LikeDogResponse>(DiscoveryErrors.DogNotFound);
        if (await dbContext.DogPasses.AnyAsync(x => x.SourceDogId == source.Id && x.TargetDogId == target.Id, cancellationToken))
            return Result.Failure<LikeDogResponse>(DiscoveryErrors.Invalid);

        DogLike? existingLike = await dbContext.DogLikes.SingleOrDefaultAsync(x => x.SourceDogId == source.Id && x.TargetDogId == target.Id, cancellationToken);
        if (existingLike is null) dbContext.DogLikes.Add(new DogLike(source.Id, target.Id, timeProvider.GetUtcNow()));

        bool reciprocal = await dbContext.DogLikes.AnyAsync(x => x.SourceDogId == target.Id && x.TargetDogId == source.Id, cancellationToken);
        Guid first = source.Id.CompareTo(target.Id) < 0 ? source.Id : target.Id;
        Guid second = source.Id.CompareTo(target.Id) < 0 ? target.Id : source.Id;
        DogMatch? match = await dbContext.DogMatches.SingleOrDefaultAsync(x => x.DogAId == first && x.DogBId == second, cancellationToken);
        bool created = false;
        if (reciprocal && match is null)
        {
            match = new DogMatch(source.Id, target.Id, timeProvider.GetUtcNow());
            dbContext.DogMatches.Add(match);
            dbContext.Conversations.Add(new Conversation(match.Id, timeProvider.GetUtcNow()));
            const string title = "Novo match!";
            string message = $"{source.Name} e {target.Name} curtiram um ao outro.";
            dbContext.UserNotifications.Add(new UserNotification(source.OwnerUserId, match.Id, title, message, timeProvider.GetUtcNow()));
            dbContext.UserNotifications.Add(new UserNotification(target.OwnerUserId, match.Id, title, message, timeProvider.GetUtcNow()));
            created = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success(new LikeDogResponse(created, match?.Id));
    }

    public async Task<Result> PassAsync(Guid userId, Guid targetDogId, PassDogRequest request, CancellationToken cancellationToken)
    {
        Dog? source = await dbContext.Dogs.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.SourceDogId && x.OwnerUserId == userId && x.Status == DogProfileStatus.Active, cancellationToken);
        Dog? target = await dbContext.Dogs.AsNoTracking().SingleOrDefaultAsync(x => x.Id == targetDogId && x.OwnerUserId != userId && x.Status == DogProfileStatus.Active && dbContext.TutorProfiles.Any(t => t.UserId == x.OwnerUserId && t.AllowDiscovery), cancellationToken);
        if (source is null || target is null) return Result.Failure(DiscoveryErrors.DogNotFound);
        if (await IsBlockedAsync(userId, target.OwnerUserId, cancellationToken)) return Result.Failure(DiscoveryErrors.DogNotFound);
        if (!await dbContext.DogPasses.AnyAsync(x => x.SourceDogId == source.Id && x.TargetDogId == target.Id, cancellationToken))
        {
            dbContext.DogPasses.Add(new DogPass(source.Id, target.Id, timeProvider.GetUtcNow()));
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        return Result.Success();
    }

    public async Task<Result<DiscoveryImage>> GetPrimaryPhotoAsync(Guid userId, Guid dogId, CancellationToken cancellationToken)
    {
        var photo = await (
            from dog in dbContext.Dogs.AsNoTracking()
            join profile in dbContext.TutorProfiles.AsNoTracking() on dog.OwnerUserId equals profile.UserId
            join image in dbContext.DogPhotos.AsNoTracking() on dog.Id equals image.DogId
            where dog.Id == dogId && dog.Status == DogProfileStatus.Active && profile.AllowDiscovery && image.IsPrimary &&
                !dbContext.BlockedUsers.Any(b => (b.UserId == userId && b.BlockedUserId == dog.OwnerUserId) || (b.UserId == dog.OwnerUserId && b.BlockedUserId == userId))
            select new { image.StorageKey, image.ContentType })
            .SingleOrDefaultAsync(cancellationToken);
        if (photo is null) return Result.Failure<DiscoveryImage>(DiscoveryErrors.DogNotFound);
        string path = Path.Combine(environment.ContentRootPath, ".dev-storage", photo.StorageKey);
        if (!File.Exists(path)) return Result.Failure<DiscoveryImage>(DiscoveryErrors.DogNotFound);
        return Result.Success(new DiscoveryImage(await File.ReadAllBytesAsync(path, cancellationToken), photo.ContentType));
    }

    private Task<bool> IsBlockedAsync(Guid firstUserId, Guid secondUserId, CancellationToken cancellationToken) =>
        dbContext.BlockedUsers.AnyAsync(x => (x.UserId == firstUserId && x.BlockedUserId == secondUserId) || (x.UserId == secondUserId && x.BlockedUserId == firstUserId), cancellationToken);
}
