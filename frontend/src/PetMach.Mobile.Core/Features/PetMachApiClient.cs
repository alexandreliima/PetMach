using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PetMach.Mobile.Core.Identity;

namespace PetMach.Mobile.Core.Features;

public sealed class PetMachApiClient(HttpClient httpClient, AuthenticationSession session) : IPetMachApiClient
{
    public async Task<TutorProfileModel?> GetTutorProfileAsync(CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await SendAsync(HttpMethod.Get, "api/v1/tutors/me", null, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        return await ReadAsync<TutorProfileModel>(response, cancellationToken);
    }

    public async Task<TutorProfileModel> SaveTutorProfileAsync(TutorProfileInput input, CancellationToken cancellationToken) =>
        await SendAndReadAsync<TutorProfileModel>(HttpMethod.Put, "api/v1/tutors/me", JsonContent.Create(input), cancellationToken);

    public async Task<IReadOnlyCollection<DogModel>> GetDogsAsync(CancellationToken cancellationToken) =>
        await SendAndReadAsync<DogModel[]>(HttpMethod.Get, "api/v1/dogs", null, cancellationToken);

    public async Task<DogModel> CreateDogAsync(DogInput input, CancellationToken cancellationToken) =>
        await SendAndReadAsync<DogModel>(HttpMethod.Post, "api/v1/dogs", JsonContent.Create(input), cancellationToken);

    public async Task<DogPhotoModel> UploadDogPhotoAsync(Guid dogId, PickedFile file, CancellationToken cancellationToken) =>
        await UploadAsync<DogPhotoModel>($"api/v1/dogs/{dogId}/photos", file, cancellationToken);

    public async Task<DogHealthModel> GetDogHealthAsync(Guid dogId, CancellationToken cancellationToken) =>
        await SendAndReadAsync<DogHealthModel>(HttpMethod.Get, $"api/v1/dogs/{dogId}/health", null, cancellationToken);

    public async Task<VaccinationModel> AddVaccinationAsync(Guid dogId, string name, DateOnly appliedOn, DateOnly? nextDoseOn, CancellationToken cancellationToken) =>
        await SendAndReadAsync<VaccinationModel>(HttpMethod.Post, $"api/v1/dogs/{dogId}/health/vaccinations", JsonContent.Create(new { VaccineName = name, AppliedOn = appliedOn, NextDoseOn = nextDoseOn }), cancellationToken);

    public async Task UploadVaccinationProofAsync(Guid dogId, Guid vaccinationId, PickedFile file, CancellationToken cancellationToken) =>
        _ = await UploadAsync<object>($"api/v1/dogs/{dogId}/health/vaccinations/{vaccinationId}/proof", file, cancellationToken);

    public async Task<DewormingModel> AddDewormingAsync(Guid dogId, string productName, DateOnly appliedOn, DateOnly? nextDoseOn, CancellationToken cancellationToken) =>
        await SendAndReadAsync<DewormingModel>(HttpMethod.Post, $"api/v1/dogs/{dogId}/health/dewormings", JsonContent.Create(new { ProductName = productName, AppliedOn = appliedOn, NextDoseOn = nextDoseOn }), cancellationToken);

    public async Task<DiscoveryPageModel> DiscoverAsync(Guid sourceDogId, DiscoveryFilterModel filter, CancellationToken cancellationToken)
    {
        List<string> query = [$"sourceDogId={sourceDogId}", $"page={filter.Page}", $"pageSize={filter.PageSize}"];
        Add(query, "sex", filter.Sex);
        Add(query, "size", filter.Size);
        if (!string.IsNullOrWhiteSpace(filter.Breed)) query.Add($"breed={Uri.EscapeDataString(filter.Breed.Trim())}");
        Add(query, "energyLevel", filter.EnergyLevel);
        Add(query, "goal", filter.Goal);
        Add(query, "neutered", filter.Neutered);
        Add(query, "vaccinationUpToDate", filter.VaccinationUpToDate);
        return await SendAndReadAsync<DiscoveryPageModel>(HttpMethod.Get, $"api/v1/discovery?{string.Join('&', query)}", null, cancellationToken);
    }

    public async Task<LikeDogModel> LikeAsync(Guid sourceDogId, Guid targetDogId, CancellationToken cancellationToken) =>
        await SendAndReadAsync<LikeDogModel>(HttpMethod.Post, $"api/v1/dogs/{targetDogId}/likes", JsonContent.Create(new { SourceDogId = sourceDogId }), cancellationToken);

    public async Task PassAsync(Guid sourceDogId, Guid targetDogId, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await SendAsync(HttpMethod.Post, $"api/v1/dogs/{targetDogId}/passes", JsonContent.Create(new { SourceDogId = sourceDogId }), cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyCollection<MatchModel>> GetMatchesAsync(CancellationToken cancellationToken) =>
        await SendAndReadAsync<MatchModel[]>(HttpMethod.Get, "api/v1/matches", null, cancellationToken);

    public async Task EndMatchAsync(Guid matchId, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await SendAsync(HttpMethod.Delete, $"api/v1/matches/{matchId}", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task BlockDogOwnerAsync(Guid targetDogId, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await SendAsync(HttpMethod.Post, $"api/v1/dogs/{targetDogId}/block-owner", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyCollection<NotificationModel>> GetNotificationsAsync(CancellationToken cancellationToken) =>
        await SendAndReadAsync<NotificationModel[]>(HttpMethod.Get, "api/v1/notifications", null, cancellationToken);

    public async Task MarkNotificationAsReadAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await SendAsync(HttpMethod.Put, $"api/v1/notifications/{notificationId}/read", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyCollection<ConversationModel>> GetConversationsAsync(CancellationToken cancellationToken) =>
        await SendAndReadAsync<ConversationModel[]>(HttpMethod.Get, "api/v1/chat/conversations", null, cancellationToken);

    public async Task<ChatMessagePageModel> GetMessagesAsync(Guid conversationId, int page, CancellationToken cancellationToken) =>
        await SendAndReadAsync<ChatMessagePageModel>(HttpMethod.Get, $"api/v1/chat/conversations/{conversationId}/messages?page={page}&pageSize=30", null, cancellationToken);

    public async Task<ChatMessageModel> SendMessageAsync(Guid conversationId, string content, CancellationToken cancellationToken) =>
        await SendAndReadAsync<ChatMessageModel>(HttpMethod.Post, $"api/v1/chat/conversations/{conversationId}/messages", JsonContent.Create(new { Content = content }), cancellationToken);

    public async Task<ConversationReadModel> MarkConversationReadAsync(Guid conversationId, Guid messageId, CancellationToken cancellationToken) =>
        await SendAndReadAsync<ConversationReadModel>(HttpMethod.Put, $"api/v1/chat/conversations/{conversationId}/read", JsonContent.Create(new { MessageId = messageId }), cancellationToken);

    public async Task<IReadOnlyCollection<MeetingModel>> GetMeetingsAsync(CancellationToken cancellationToken) =>
        await SendAndReadAsync<MeetingModel[]>(HttpMethod.Get, "api/v1/meetings", null, cancellationToken);

    public async Task<MeetingModel> CreateMeetingAsync(Guid matchId, DateTimeOffset scheduledAtUtc, string placeName, string? notes, CancellationToken cancellationToken) =>
        await SendAndReadAsync<MeetingModel>(HttpMethod.Post, "api/v1/meetings", JsonContent.Create(new { MatchId = matchId, ScheduledAtUtc = scheduledAtUtc, PlaceName = placeName, Notes = notes }), cancellationToken);

    public async Task<MeetingModel> TransitionMeetingAsync(Guid meetingId, string transition, CancellationToken cancellationToken) =>
        await SendAndReadAsync<MeetingModel>(HttpMethod.Put, $"api/v1/meetings/{meetingId}/{transition}", null, cancellationToken);

    public async Task<IReadOnlyCollection<PartnerSpaceModel>> GetPartnerSpacesAsync(string? city, string? state, CancellationToken cancellationToken)
    {
        List<string> query = [];
        if (!string.IsNullOrWhiteSpace(city)) query.Add($"city={Uri.EscapeDataString(city.Trim())}");
        if (!string.IsNullOrWhiteSpace(state)) query.Add($"state={Uri.EscapeDataString(state.Trim())}");
        string suffix = query.Count == 0 ? string.Empty : $"?{string.Join('&', query)}";
        return await SendAndReadAsync<PartnerSpaceModel[]>(HttpMethod.Get, $"api/v1/spaces{suffix}", null, cancellationToken);
    }

    public async Task<IReadOnlyCollection<SpaceAvailabilityModel>> GetSpaceAvailabilityAsync(Guid spaceId, CancellationToken cancellationToken) =>
        await SendAndReadAsync<SpaceAvailabilityModel[]>(HttpMethod.Get, $"api/v1/spaces/{spaceId}/availability", null, cancellationToken);

    public async Task<IReadOnlyCollection<ReservationModel>> GetReservationsAsync(CancellationToken cancellationToken) =>
        await SendAndReadAsync<ReservationModel[]>(HttpMethod.Get, "api/v1/reservations", null, cancellationToken);

    public async Task<ReservationModel> CreateReservationAsync(Guid availabilityId, Guid dogId, CancellationToken cancellationToken) =>
        await SendAndReadAsync<ReservationModel>(HttpMethod.Post, "api/v1/reservations", JsonContent.Create(new { AvailabilityId = availabilityId, DogId = dogId }), cancellationToken);

    public async Task<ReservationModel> CancelReservationAsync(Guid reservationId, CancellationToken cancellationToken) =>
        await SendAndReadAsync<ReservationModel>(HttpMethod.Put, $"api/v1/reservations/{reservationId}/cancel", null, cancellationToken);

    public async Task<PartnerManagementModel> GetManagedPartnerAsync(CancellationToken cancellationToken) =>
        await SendAndReadAsync<PartnerManagementModel>(HttpMethod.Get, "api/v1/partners/me", null, cancellationToken);

    public async Task<IReadOnlyCollection<PartnerSpaceModel>> GetManagedPartnerSpacesAsync(CancellationToken cancellationToken) =>
        await SendAndReadAsync<PartnerSpaceModel[]>(HttpMethod.Get, "api/v1/partners/me/spaces", null, cancellationToken);

    public async Task<SpaceAvailabilityModel> CreateSpaceAvailabilityAsync(Guid spaceId, DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc, CancellationToken cancellationToken) =>
        await SendAndReadAsync<SpaceAvailabilityModel>(HttpMethod.Post, $"api/v1/partners/spaces/{spaceId}/availability", JsonContent.Create(new { StartsAtUtc = startsAtUtc, EndsAtUtc = endsAtUtc }), cancellationToken);

    public async Task<IReadOnlyCollection<ReservationModel>> GetPartnerReservationsAsync(CancellationToken cancellationToken) =>
        await SendAndReadAsync<ReservationModel[]>(HttpMethod.Get, "api/v1/partners/reservations", null, cancellationToken);

    public async Task<ReservationModel> TransitionPartnerReservationAsync(Guid reservationId, string transition, bool paymentReceivedOnSite, CancellationToken cancellationToken) =>
        await SendAndReadAsync<ReservationModel>(HttpMethod.Put, $"api/v1/partners/reservations/{reservationId}/{transition}", transition == "complete" ? JsonContent.Create(new { PaymentReceivedOnSite = paymentReceivedOnSite }) : null, cancellationToken);

    public async Task<IReadOnlyCollection<AdoptionProfileModel>> GetAdoptionProfilesAsync(CancellationToken cancellationToken) =>
        await SendAndReadAsync<AdoptionProfileModel[]>(HttpMethod.Get, "api/v1/adoption", null, cancellationToken);

    public async Task<AdoptionProfileModel> CreateAdoptionProfileAsync(Guid dogId, string story, string requirements, CancellationToken cancellationToken) =>
        await SendAndReadAsync<AdoptionProfileModel>(HttpMethod.Post, "api/v1/adoption", JsonContent.Create(new { DogId = dogId, Story = story, Requirements = requirements, TermsAccepted = true }), cancellationToken);

    public async Task SuspendAdoptionProfileAsync(Guid profileId, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await SendAsync(HttpMethod.Put, $"api/v1/adoption/{profileId}/suspend", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<AdoptionApplicationModel> ApplyForAdoptionAsync(Guid profileId, string motivation, string experience, string housingContext, CancellationToken cancellationToken) =>
        await SendAndReadAsync<AdoptionApplicationModel>(HttpMethod.Post, $"api/v1/adoption/{profileId}/applications", JsonContent.Create(new { Motivation = motivation, Experience = experience, HousingContext = housingContext, TermsAccepted = true }), cancellationToken);

    public async Task<IReadOnlyCollection<AdoptionApplicationModel>> GetMyAdoptionApplicationsAsync(CancellationToken cancellationToken) =>
        await SendAndReadAsync<AdoptionApplicationModel[]>(HttpMethod.Get, "api/v1/adoption/applications", null, cancellationToken);

    public async Task<AdoptionApplicationModel> TransitionAdoptionApplicationAsync(Guid applicationId, string transition, CancellationToken cancellationToken) =>
        await SendAndReadAsync<AdoptionApplicationModel>(HttpMethod.Put, $"api/v1/adoption/applications/{applicationId}/{transition}", null, cancellationToken);

    public async Task<ReportModel> ReportAdoptionProfileAsync(Guid profileId, string reason, string description, CancellationToken cancellationToken) =>
        await SendAndReadAsync<ReportModel>(HttpMethod.Post, "api/v1/reports", JsonContent.Create(new { TargetType = 2, TargetId = profileId, Reason = ReasonValue(reason), Description = description }), cancellationToken);

    public async Task<ReportEvidenceModel> UploadReportEvidenceAsync(Guid reportId, PickedFile file, CancellationToken cancellationToken) =>
        await UploadAsync<ReportEvidenceModel>($"api/v1/reports/{reportId}/evidence", file, cancellationToken);

    private async Task<T> UploadAsync<T>(string path, PickedFile file, CancellationToken cancellationToken)
    {
        using MultipartFormDataContent content = new();
        using ByteArrayContent fileContent = new(file.Content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "file", file.FileName);
        return await SendAndReadAsync<T>(HttpMethod.Post, path, content, cancellationToken);
    }

    private static int ReasonValue(string reason) => reason switch
    {
        "Harassment" => 0,
        "Fraud" => 1,
        "UnsafeContent" => 2,
        "AnimalWelfare" => 3,
        "Spam" => 4,
        _ => 5,
    };

    private async Task<T> SendAndReadAsync<T>(HttpMethod method, string path, HttpContent? content, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await SendAsync(method, path, content, cancellationToken);
        return await ReadAsync<T>(response, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, HttpContent? content, CancellationToken cancellationToken)
    {
        string? token = await session.GetAccessTokenAsync(cancellationToken);
        if (token is null)
        {
            throw new AuthenticationRequiredException();
        }

        BufferedContent? bufferedContent = await BufferAsync(content, cancellationToken);
        HttpResponseMessage response = await SendOnceAsync(
            method,
            path,
            bufferedContent,
            token,
            cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        response.Dispose();
        string? refreshedToken = await session.RefreshAfterUnauthorizedAsync(token, cancellationToken);
        if (refreshedToken is null)
        {
            throw new AuthenticationRequiredException();
        }

        return await SendOnceAsync(
            method,
            path,
            bufferedContent,
            refreshedToken,
            cancellationToken);
    }

    private async Task<HttpResponseMessage> SendOnceAsync(
        HttpMethod method,
        string path,
        BufferedContent? content,
        string accessToken,
        CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (content is not null)
        {
            ByteArrayContent requestContent = new(content.Body);
            foreach ((string name, string[] values) in content.Headers)
            {
                requestContent.Headers.TryAddWithoutValidation(name, values);
            }

            request.Content = requestContent;
        }

        return await httpClient.SendAsync(request, cancellationToken);
    }

    private static async Task<BufferedContent?> BufferAsync(
        HttpContent? content,
        CancellationToken cancellationToken)
    {
        if (content is null)
        {
            return null;
        }

        using (content)
        {
            byte[] body = await content.ReadAsByteArrayAsync(cancellationToken);
            Dictionary<string, string[]> headers = content.Headers.ToDictionary(
                header => header.Key,
                header => header.Value.ToArray(),
                StringComparer.OrdinalIgnoreCase);
            return new BufferedContent(body, headers);
        }
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken)
            ?? throw new InvalidOperationException("A API retornou uma resposta vazia.");
    }

    private static void Add<T>(List<string> query, string name, T? value) where T : struct
    {
        if (value.HasValue) query.Add($"{name}={value.Value.ToString()!.ToLowerInvariant()}");
    }

    private sealed record BufferedContent(
        byte[] Body,
        IReadOnlyDictionary<string, string[]> Headers);
}
