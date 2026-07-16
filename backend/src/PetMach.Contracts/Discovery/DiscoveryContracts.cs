using PetMach.Contracts.Dogs;

namespace PetMach.Contracts.Discovery;

public sealed record DiscoveryFilterRequest(
    Guid SourceDogId,
    DogSexContract? Sex,
    DogSizeContract? Size,
    string? Breed,
    EnergyLevelContract? EnergyLevel,
    DogGoalContract? Goal,
    bool? Neutered,
    bool? VaccinationUpToDate,
    int Page = 1,
    int PageSize = 20);

public sealed record DiscoveryDogResponse(
    Guid DogId,
    string Name,
    DateOnly? BirthDate,
    bool ApproximateAge,
    DogSexContract Sex,
    string Breed,
    DogSizeContract Size,
    string Temperament,
    EnergyLevelContract EnergyLevel,
    DogGoalContract Goal,
    bool Neutered,
    bool VaccinationUpToDate,
    string Region,
    string? PrimaryPhotoUrl);

public sealed record DiscoveryPageResponse(IReadOnlyCollection<DiscoveryDogResponse> Items, int Page, int PageSize, bool HasMore);
public sealed record LikeDogRequest(Guid SourceDogId);
public sealed record LikeDogResponse(bool MatchCreated, Guid? MatchId);
public sealed record PassDogRequest(Guid SourceDogId);
