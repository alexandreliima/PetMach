using FluentAssertions;
using PetMach.Mobile.Core.Features;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Tests.Features;

public sealed class TutorProfileViewModelTests
{
    [Fact]
    public async Task SuccessfulSaveShouldNavigateHome()
    {
        Navigator navigator = new();
        TutorProfileViewModel viewModel = new(new ProfileApi(), navigator)
        {
            FirstName = "Ana",
            LastName = "Silva",
            City = "Lisboa",
            State = "PT",
        };

        await viewModel.SaveCommand.ExecuteAsync(null);

        navigator.Route.Should().Be("//app/network");
    }

    private sealed class ProfileApi : IPetMachApiClient
    {
        public Task<TutorProfileModel> SaveTutorProfileAsync(TutorProfileInput input, CancellationToken cancellationToken) =>
            Task.FromResult(new TutorProfileModel(Guid.NewGuid(), input.FirstName, input.LastName, input.Phone, input.City, input.State, input.Biography, input.ShowCity, input.AllowDiscovery, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));
        public Task<TutorProfileModel?> GetTutorProfileAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<DogModel>> GetDogsAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<DogModel> CreateDogAsync(DogInput input, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<DogPhotoModel> UploadDogPhotoAsync(Guid dogId, PickedFile file, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<DogHealthModel> GetDogHealthAsync(Guid dogId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<VaccinationModel> AddVaccinationAsync(Guid dogId, string name, DateOnly appliedOn, DateOnly? nextDoseOn, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task UploadVaccinationProofAsync(Guid dogId, Guid vaccinationId, PickedFile file, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<DewormingModel> AddDewormingAsync(Guid dogId, string productName, DateOnly appliedOn, DateOnly? nextDoseOn, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<DiscoveryPageModel> DiscoverAsync(Guid sourceDogId, DiscoveryFilterModel filter, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<LikeDogModel> LikeAsync(Guid sourceDogId, Guid targetDogId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task PassAsync(Guid sourceDogId, Guid targetDogId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<MatchModel>> GetMatchesAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task EndMatchAsync(Guid matchId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task BlockDogOwnerAsync(Guid targetDogId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<NotificationModel>> GetNotificationsAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task MarkNotificationAsReadAsync(Guid notificationId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<ConversationModel>> GetConversationsAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<ChatMessagePageModel> GetMessagesAsync(Guid conversationId, int page, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<ChatMessageModel> SendMessageAsync(Guid conversationId, string content, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<ConversationReadModel> MarkConversationReadAsync(Guid conversationId, Guid messageId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<MeetingModel>> GetMeetingsAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<MeetingModel> CreateMeetingAsync(Guid matchId, DateTimeOffset scheduledAtUtc, string placeName, string? notes, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<MeetingModel> TransitionMeetingAsync(Guid meetingId, string transition, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<PartnerSpaceModel>> GetPartnerSpacesAsync(string? city, string? state, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<SpaceAvailabilityModel>> GetSpaceAvailabilityAsync(Guid spaceId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<ReservationModel>> GetReservationsAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<ReservationModel> CreateReservationAsync(Guid availabilityId, Guid dogId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<ReservationModel> CancelReservationAsync(Guid reservationId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<PartnerManagementModel> GetManagedPartnerAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<PartnerSpaceModel>> GetManagedPartnerSpacesAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<SpaceAvailabilityModel> CreateSpaceAvailabilityAsync(Guid spaceId, DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<ReservationModel>> GetPartnerReservationsAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<ReservationModel> TransitionPartnerReservationAsync(Guid reservationId, string transition, bool paymentReceivedOnSite, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<AdoptionProfileModel>> GetAdoptionProfilesAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<AdoptionProfileModel> CreateAdoptionProfileAsync(Guid dogId, string story, string requirements, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task SuspendAdoptionProfileAsync(Guid profileId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<AdoptionApplicationModel> ApplyForAdoptionAsync(Guid profileId, string motivation, string experience, string housingContext, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<AdoptionApplicationModel>> GetMyAdoptionApplicationsAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<AdoptionApplicationModel> TransitionAdoptionApplicationAsync(Guid applicationId, string transition, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<ReportModel> ReportAdoptionProfileAsync(Guid profileId, string reason, string description, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<ReportEvidenceModel> UploadReportEvidenceAsync(Guid reportId, PickedFile file, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class Navigator : IMobileNavigator
    {
        public string? Route { get; private set; }
        public Task GoToAsync(string route) { Route = route; return Task.CompletedTask; }
    }
}
