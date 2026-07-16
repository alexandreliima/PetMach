namespace PetMach.Contracts.Matches;

public sealed record MatchResponse(Guid Id, Guid MyDogId, Guid OtherDogId, string OtherDogName, string OtherDogBreed, DateTimeOffset CreatedAtUtc);
