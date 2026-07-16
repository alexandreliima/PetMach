namespace PetMach.Contracts.Adoption;

public sealed record CreateAdoptionProfileRequest(Guid DogId, string Story, string Requirements, bool TermsAccepted);
public sealed record AdoptionProfileResponse(Guid Id, Guid DogId, string DogName, string Breed, string Size, string Region, string Story, string Requirements, string Status, DateTimeOffset CreatedAtUtc, bool IsMine);
public sealed record CreateAdoptionApplicationRequest(string Motivation, string Experience, string HousingContext, bool TermsAccepted);
public sealed record AdoptionApplicationResponse(Guid Id, Guid ProfileId, string DogName, string ApplicantName, string Motivation, string Experience, string HousingContext, string Status, DateTimeOffset CreatedAtUtc, bool IsMine);
public sealed record AdoptionApplicationHistoryResponse(string? FromStatus, string ToStatus, DateTimeOffset OccurredAtUtc);
