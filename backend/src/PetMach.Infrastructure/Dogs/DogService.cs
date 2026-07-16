using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PetMach.Application.Dogs;
using PetMach.Contracts.Dogs;
using PetMach.Domain.Dogs;
using PetMach.Domain.SharedKernel;
using PetMach.Infrastructure.Persistence;

namespace PetMach.Infrastructure.Dogs;

internal sealed class DogService(PetMachDbContext dbContext, IValidator<UpsertDogRequest> validator, TimeProvider timeProvider) : IDogService
{
    public async Task<IReadOnlyCollection<DogResponse>> ListAsync(Guid userId, CancellationToken cancellationToken) =>
        (await dbContext.Dogs.AsNoTracking().Where(x => x.OwnerUserId == userId && x.Status != DogProfileStatus.Removed).OrderBy(x => x.Name).ToListAsync(cancellationToken)).Select(Map).ToArray();

    public async Task<Result<DogResponse>> GetAsync(Guid userId, Guid dogId, CancellationToken cancellationToken)
    {
        Dog? dog = await Owned(userId, dogId).AsNoTracking().SingleOrDefaultAsync(cancellationToken);
        return dog is null ? Result.Failure<DogResponse>(DogErrors.NotFound) : Result.Success(Map(dog));
    }

    public async Task<Result<DogResponse>> CreateAsync(Guid userId, UpsertDogRequest request, CancellationToken cancellationToken)
    {
        if (!(await validator.ValidateAsync(request, cancellationToken)).IsValid) return Result.Failure<DogResponse>(DogErrors.Invalid);
        DateTimeOffset now = timeProvider.GetUtcNow();
        Result<Dog> result = Dog.Create(userId, request.Name, request.BirthDate, request.ApproximateAge, (DogSex)request.Sex, request.Breed, (DogSize)request.Size, request.WeightKg, request.Neutered, request.Temperament, (EnergyLevel)request.EnergyLevel, request.SociabilityWithDogs, request.SociabilityWithPeople, request.SociabilityWithChildren, request.Restrictions, request.SpecialNeeds, request.Biography, (DogGoal)request.Goal, now);
        if (result.IsFailure) return Result.Failure<DogResponse>(result.Error);
        result.Value.Activate(); dbContext.Dogs.Add(result.Value); await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(Map(result.Value));
    }

    public async Task<Result<DogResponse>> UpdateAsync(Guid userId, Guid dogId, UpsertDogRequest request, CancellationToken cancellationToken)
    {
        if (!(await validator.ValidateAsync(request, cancellationToken)).IsValid) return Result.Failure<DogResponse>(DogErrors.Invalid);
        Dog? dog = await Owned(userId, dogId).SingleOrDefaultAsync(cancellationToken);
        if (dog is null) return Result.Failure<DogResponse>(DogErrors.NotFound);
        Result result = dog.Update(request.Name, request.BirthDate, request.ApproximateAge, (DogSex)request.Sex, request.Breed, (DogSize)request.Size, request.WeightKg, request.Neutered, request.Temperament, (EnergyLevel)request.EnergyLevel, request.SociabilityWithDogs, request.SociabilityWithPeople, request.SociabilityWithChildren, request.Restrictions, request.SpecialNeeds, request.Biography, (DogGoal)request.Goal, timeProvider.GetUtcNow());
        if (result.IsFailure) return Result.Failure<DogResponse>(result.Error);
        await dbContext.SaveChangesAsync(cancellationToken); return Result.Success(Map(dog));
    }

    public async Task<Result> DeleteAsync(Guid userId, Guid dogId, CancellationToken cancellationToken)
    {
        Dog? dog = await Owned(userId, dogId).SingleOrDefaultAsync(cancellationToken);
        if (dog is null) return Result.Failure(DogErrors.NotFound);
        dog.Remove(); await dbContext.SaveChangesAsync(cancellationToken); return Result.Success();
    }

    private IQueryable<Dog> Owned(Guid userId, Guid dogId) => dbContext.Dogs.Where(x => x.Id == dogId && x.OwnerUserId == userId && x.Status != DogProfileStatus.Removed);
    private static DogResponse Map(Dog x) => new(x.Id, x.Name, x.BirthDate, x.ApproximateAge, (DogSexContract)x.Sex, x.Breed, (DogSizeContract)x.Size, x.WeightKg, x.Neutered, x.Temperament, (EnergyLevelContract)x.EnergyLevel, x.SociabilityWithDogs, x.SociabilityWithPeople, x.SociabilityWithChildren, x.Restrictions, x.SpecialNeeds, x.Biography, (DogGoalContract)x.Goal, (DogProfileStatusContract)x.Status);
}
