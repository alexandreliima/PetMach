using PetMach.Domain.SharedKernel;

namespace PetMach.Domain.Dogs;

public sealed class Dog
{
    private Dog() { }

    public Guid Id { get; private set; }
    public Guid OwnerUserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateOnly? BirthDate { get; private set; }
    public bool ApproximateAge { get; private set; }
    public DogSex Sex { get; private set; }
    public string Breed { get; private set; } = string.Empty;
    public DogSize Size { get; private set; }
    public decimal? WeightKg { get; private set; }
    public bool Neutered { get; private set; }
    public string Temperament { get; private set; } = string.Empty;
    public EnergyLevel EnergyLevel { get; private set; }
    public int SociabilityWithDogs { get; private set; }
    public int SociabilityWithPeople { get; private set; }
    public int SociabilityWithChildren { get; private set; }
    public string? Restrictions { get; private set; }
    public string? SpecialNeeds { get; private set; }
    public string? Biography { get; private set; }
    public DogGoal Goal { get; private set; }
    public DogProfileStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Result<Dog> Create(Guid ownerUserId, string name, DateOnly? birthDate, bool approximateAge, DogSex sex, string breed, DogSize size, decimal? weightKg, bool neutered, string temperament, EnergyLevel energyLevel, int dogs, int people, int children, string? restrictions, string? specialNeeds, string? biography, DogGoal goal, DateTimeOffset now)
    {
        Dog dog = new() { Id = Guid.NewGuid(), OwnerUserId = ownerUserId, CreatedAtUtc = now, Status = DogProfileStatus.Draft };
        Result updated = dog.Update(name, birthDate, approximateAge, sex, breed, size, weightKg, neutered, temperament, energyLevel, dogs, people, children, restrictions, specialNeeds, biography, goal, now);
        return updated.IsSuccess ? Result.Success(dog) : Result.Failure<Dog>(updated.Error);
    }

    public Result Update(string name, DateOnly? birthDate, bool approximateAge, DogSex sex, string breed, DogSize size, decimal? weightKg, bool neutered, string temperament, EnergyLevel energyLevel, int dogs, int people, int children, string? restrictions, string? specialNeeds, string? biography, DogGoal goal, DateTimeOffset now)
    {
        if (OwnerUserId == Guid.Empty || string.IsNullOrWhiteSpace(name) || name.Length > 100 || string.IsNullOrWhiteSpace(breed) || breed.Length > 120 || string.IsNullOrWhiteSpace(temperament) || temperament.Length > 500 || birthDate > DateOnly.FromDateTime(now.UtcDateTime) || weightKg is <= 0 or > 150 || !ScoreValid(dogs) || !ScoreValid(people) || !ScoreValid(children) || restrictions?.Length > 1000 || specialNeeds?.Length > 1000 || biography?.Length > 2000)
            return Result.Failure(DogErrors.Invalid);
        Name = name.Trim(); BirthDate = birthDate; ApproximateAge = approximateAge; Sex = sex; Breed = breed.Trim(); Size = size;
        WeightKg = weightKg; Neutered = neutered; Temperament = temperament.Trim(); EnergyLevel = energyLevel;
        SociabilityWithDogs = dogs; SociabilityWithPeople = people; SociabilityWithChildren = children;
        Restrictions = Clean(restrictions); SpecialNeeds = Clean(specialNeeds); Biography = Clean(biography); Goal = goal; UpdatedAtUtc = now;
        return Result.Success();
    }

    public void Activate() { if (Status == DogProfileStatus.Draft || Status == DogProfileStatus.Hidden) Status = DogProfileStatus.Active; }
    public void Remove() => Status = DogProfileStatus.Removed;
    private static bool ScoreValid(int value) => value is >= 1 and <= 5;
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
