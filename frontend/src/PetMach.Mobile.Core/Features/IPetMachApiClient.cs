namespace PetMach.Mobile.Core.Features;

public interface IPetMachApiClient
{
    Task<TutorProfileModel?> GetTutorProfileAsync(CancellationToken cancellationToken);
    Task<TutorProfileModel> SaveTutorProfileAsync(TutorProfileInput input, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DogModel>> GetDogsAsync(CancellationToken cancellationToken);
    Task<DogModel> CreateDogAsync(DogInput input, CancellationToken cancellationToken);
    Task<DogPhotoModel> UploadDogPhotoAsync(Guid dogId, PickedFile file, CancellationToken cancellationToken);
    Task<DogHealthModel> GetDogHealthAsync(Guid dogId, CancellationToken cancellationToken);
    Task<VaccinationModel> AddVaccinationAsync(Guid dogId, string name, DateOnly appliedOn, DateOnly? nextDoseOn, CancellationToken cancellationToken);
    Task UploadVaccinationProofAsync(Guid dogId, Guid vaccinationId, PickedFile file, CancellationToken cancellationToken);
    Task<DewormingModel> AddDewormingAsync(Guid dogId, string productName, DateOnly appliedOn, DateOnly? nextDoseOn, CancellationToken cancellationToken);
    Task<DiscoveryPageModel> DiscoverAsync(Guid sourceDogId, DiscoveryFilterModel filter, CancellationToken cancellationToken);
    Task<LikeDogModel> LikeAsync(Guid sourceDogId, Guid targetDogId, CancellationToken cancellationToken);
    Task PassAsync(Guid sourceDogId, Guid targetDogId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<MatchModel>> GetMatchesAsync(CancellationToken cancellationToken);
    Task EndMatchAsync(Guid matchId, CancellationToken cancellationToken);
    Task BlockDogOwnerAsync(Guid targetDogId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<NotificationModel>> GetNotificationsAsync(CancellationToken cancellationToken);
    Task MarkNotificationAsReadAsync(Guid notificationId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ConversationModel>> GetConversationsAsync(CancellationToken cancellationToken);
    Task<ChatMessagePageModel> GetMessagesAsync(Guid conversationId, int page, CancellationToken cancellationToken);
    Task<ChatMessageModel> SendMessageAsync(Guid conversationId, string content, CancellationToken cancellationToken);
    Task<ConversationReadModel> MarkConversationReadAsync(Guid conversationId, Guid messageId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<MeetingModel>> GetMeetingsAsync(CancellationToken cancellationToken);
    Task<MeetingModel> CreateMeetingAsync(Guid matchId, DateTimeOffset scheduledAtUtc, string placeName, string? notes, CancellationToken cancellationToken);
    Task<MeetingModel> TransitionMeetingAsync(Guid meetingId, string transition, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PartnerSpaceModel>> GetPartnerSpacesAsync(string? city, string? state, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SpaceAvailabilityModel>> GetSpaceAvailabilityAsync(Guid spaceId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ReservationModel>> GetReservationsAsync(CancellationToken cancellationToken);
    Task<ReservationModel> CreateReservationAsync(Guid availabilityId, Guid dogId, CancellationToken cancellationToken);
    Task<ReservationModel> CancelReservationAsync(Guid reservationId, CancellationToken cancellationToken);
    Task<PartnerManagementModel> GetManagedPartnerAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PartnerSpaceModel>> GetManagedPartnerSpacesAsync(CancellationToken cancellationToken);
    Task<SpaceAvailabilityModel> CreateSpaceAvailabilityAsync(Guid spaceId, DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ReservationModel>> GetPartnerReservationsAsync(CancellationToken cancellationToken);
    Task<ReservationModel> TransitionPartnerReservationAsync(Guid reservationId, string transition, bool paymentReceivedOnSite, CancellationToken cancellationToken);
}

public interface IDeviceFilePicker
{
    Task<PickedFile?> PickPhotoAsync(CancellationToken cancellationToken);
    Task<PickedFile?> PickHealthProofAsync(CancellationToken cancellationToken);
}
