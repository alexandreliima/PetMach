namespace PetMach.Mobile.Core.Features;

public sealed record TutorProfileInput(string FirstName, string LastName, string? Phone, string City, string State, string? Biography, bool ShowCity, bool AllowDiscovery);
public sealed record TutorProfileModel(Guid Id, string FirstName, string LastName, string? Phone, string City, string State, string? Biography, bool ShowCity, bool AllowDiscovery, DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc);

public enum DogSexModel { Female, Male }
public enum DogSizeModel { Small, Medium, Large, Giant }
public enum EnergyLevelModel { Low, Moderate, High }
public enum DogGoalModel { Friendship, Socialization, Walks, Events, Adoption }
public enum DogProfileStatusModel { Draft, Active, Hidden, Suspended, Removed }

public sealed record DogInput(
    string Name,
    DateOnly? BirthDate,
    bool ApproximateAge,
    DogSexModel Sex,
    string Breed,
    DogSizeModel Size,
    decimal? WeightKg,
    bool Neutered,
    string Temperament,
    EnergyLevelModel EnergyLevel,
    int SociabilityWithDogs,
    int SociabilityWithPeople,
    int SociabilityWithChildren,
    string? Restrictions,
    string? SpecialNeeds,
    string? Biography,
    DogGoalModel Goal);

public sealed record DogModel(
    Guid Id,
    string Name,
    DateOnly? BirthDate,
    bool ApproximateAge,
    DogSexModel Sex,
    string Breed,
    DogSizeModel Size,
    decimal? WeightKg,
    bool Neutered,
    string Temperament,
    EnergyLevelModel EnergyLevel,
    int SociabilityWithDogs,
    int SociabilityWithPeople,
    int SociabilityWithChildren,
    string? Restrictions,
    string? SpecialNeeds,
    string? Biography,
    DogGoalModel Goal,
    DogProfileStatusModel Status);

public sealed record DogPhotoModel(Guid Id, Guid DogId, string ContentType, long Length, bool IsPrimary);
public sealed record VaccinationModel(Guid Id, string VaccineName, DateOnly AppliedOn, DateOnly? NextDoseOn, bool HasProof);
public sealed record DewormingModel(Guid Id, string ProductName, DateOnly AppliedOn, DateOnly? NextDoseOn);
public sealed record DogHealthModel(IReadOnlyCollection<VaccinationModel> Vaccinations, IReadOnlyCollection<DewormingModel> Dewormings, bool VaccinationUpToDate, string Disclaimer);
public sealed record DiscoveryDogModel(Guid DogId, string Name, DateOnly? BirthDate, bool ApproximateAge, DogSexModel Sex, string Breed, DogSizeModel Size, string Temperament, EnergyLevelModel EnergyLevel, DogGoalModel Goal, bool Neutered, bool VaccinationUpToDate, string Region, string? PrimaryPhotoUrl);
public sealed record DiscoveryPageModel(IReadOnlyCollection<DiscoveryDogModel> Items, int Page, int PageSize, bool HasMore);
public sealed record DiscoveryFilterModel(
    DogSexModel? Sex = null,
    DogSizeModel? Size = null,
    string? Breed = null,
    EnergyLevelModel? EnergyLevel = null,
    DogGoalModel? Goal = null,
    bool? Neutered = null,
    bool? VaccinationUpToDate = null,
    int Page = 1,
    int PageSize = 10);
public sealed record LikeDogModel(bool MatchCreated, Guid? MatchId);
public sealed record MatchModel(Guid Id, Guid MyDogId, Guid OtherDogId, string OtherDogName, string OtherDogBreed, DateTimeOffset CreatedAtUtc);
public sealed record NotificationModel(Guid Id, Guid? MatchId, Guid? MeetingId, string Type, string Title, string Message, DateTimeOffset CreatedAtUtc, DateTimeOffset? ReadAtUtc);
public sealed record ConversationModel(Guid Id, Guid MatchId, string OtherDogName, int UnreadCount, DateTimeOffset CreatedAtUtc);
public sealed record ChatMessageModel(Guid Id, Guid ConversationId, Guid SenderUserId, string Content, DateTimeOffset SentAtUtc);
public sealed record ChatMessagePageModel(IReadOnlyCollection<ChatMessageModel> Items, int Page, int PageSize, bool HasMore);
public sealed record ConversationReadModel(Guid ConversationId, Guid UserId, Guid MessageId, DateTimeOffset ReadAtUtc);
public enum MeetingStatusModel { Proposed, Accepted, Declined, Cancelled }
public sealed record MeetingModel(Guid Id, Guid MatchId, Guid ProposedByUserId, DateTimeOffset ScheduledAtUtc, string PlaceName, string? Notes, MeetingStatusModel Status, bool CanRespond, bool CanCancel, DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc);
public sealed record PartnerSpaceModel(Guid Id, Guid EstablishmentId, string EstablishmentName, string Name, string Description, int Capacity, decimal InformationalPrice, string City, string State, string TimeZoneId);
public sealed record PartnerManagementModel(Guid Id, string LegalName, string DisplayName, string RegistrationNumber, string City, string State, string TimeZoneId, bool IsActive);
public sealed record SpaceAvailabilityModel(Guid Id, Guid SpaceId, DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc, bool IsAvailable)
{
    public string Period => $"{StartsAtUtc.ToLocalTime():dd/MM HH:mm} – {EndsAtUtc.ToLocalTime():dd/MM HH:mm}";
}
public sealed record ReservationModel(Guid Id, Guid AvailabilityId, Guid SpaceId, string SpaceName, Guid DogId, string DogName, DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc, string Status, string PaymentStatus, DateTimeOffset CreatedAtUtc, DateTimeOffset? CancelledAtUtc)
{
    public string Period => $"{StartsAtUtc.ToLocalTime():dd/MM HH:mm} – {EndsAtUtc.ToLocalTime():dd/MM HH:mm}";
    public bool CanCancel => Status is "Pending" or "Confirmed";
    public string StatusLabel => Status switch { "Pending" => "Aguardando confirmação", "Confirmed" => "Confirmada", "Cancelled" => "Cancelada", "Completed" => "Concluída", "NoShow" => "Ausência registrada", _ => Status };
    public bool CanConfirm => Status == "Pending";
    public bool CanComplete => Status == "Confirmed" && StartsAtUtc <= DateTimeOffset.UtcNow;
    public bool CanMarkNoShow => Status == "Confirmed" && EndsAtUtc <= DateTimeOffset.UtcNow;
}
public sealed record AdoptionProfileModel(Guid Id, Guid DogId, string DogName, string Breed, string Size, string Region, string Story, string Requirements, string Status, DateTimeOffset CreatedAtUtc, bool IsMine)
{
    public bool CanApply => !IsMine && Status == "Available";
    public bool CanSuspend => IsMine && Status is "Available" or "InProgress";
}
public sealed record AdoptionApplicationModel(Guid Id, Guid ProfileId, string DogName, string ApplicantName, string Motivation, string Experience, string HousingContext, string Status, DateTimeOffset CreatedAtUtc, bool IsMine)
{
    public bool CanWithdraw => IsMine && Status is "Submitted" or "UnderReview";
}
public sealed record ReportModel(Guid Id, string TargetType, Guid TargetId, string Reason, string Description, string Status, DateTimeOffset CreatedAtUtc, int EvidenceCount);
public sealed record ReportEvidenceModel(Guid Id, string ContentType, long Length, DateTimeOffset CreatedAtUtc);
public sealed record PickedFile(string FileName, string ContentType, byte[] Content);

public sealed class AuthenticationRequiredException : InvalidOperationException
{
    public AuthenticationRequiredException() : base("Entre na sua conta para continuar.") { }
}
