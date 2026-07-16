namespace PetMach.Contracts.Dogs;

public enum DogSexContract { Female, Male }
public enum DogSizeContract { Small, Medium, Large, Giant }
public enum EnergyLevelContract { Low, Moderate, High }
public enum DogGoalContract { Friendship, Socialization, Walks, Events, Adoption }
public enum DogProfileStatusContract { Draft, Active, Hidden, Suspended, Removed }
public sealed record UpsertDogRequest(string Name, DateOnly? BirthDate, bool ApproximateAge, DogSexContract Sex, string Breed, DogSizeContract Size, decimal? WeightKg, bool Neutered, string Temperament, EnergyLevelContract EnergyLevel, int SociabilityWithDogs, int SociabilityWithPeople, int SociabilityWithChildren, string? Restrictions, string? SpecialNeeds, string? Biography, DogGoalContract Goal);
public sealed record DogResponse(Guid Id, string Name, DateOnly? BirthDate, bool ApproximateAge, DogSexContract Sex, string Breed, DogSizeContract Size, decimal? WeightKg, bool Neutered, string Temperament, EnergyLevelContract EnergyLevel, int SociabilityWithDogs, int SociabilityWithPeople, int SociabilityWithChildren, string? Restrictions, string? SpecialNeeds, string? Biography, DogGoalContract Goal, DogProfileStatusContract Status);
public sealed record DogPhotoResponse(Guid Id, Guid DogId, string ContentType, long Length, bool IsPrimary);
public sealed record BreedResponse(string Name);
