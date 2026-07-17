using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PetMach.Contracts.Dogs;

namespace PetMach.Api.IntegrationTests;

public sealed class DogEndpointsTests : IClassFixture<PetMachApiFactory>
{
    private readonly HttpClient client;

    public DogEndpointsTests(PetMachApiFactory factory) => client = factory.CreateClient();

    [Fact]
    public async Task BreedCatalogShouldBePublic()
    {
        HttpResponseMessage response = await client.GetAsync("/api/v1/dogs/breeds", CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        BreedResponse[]? breeds = await response.Content.ReadFromJsonAsync<BreedResponse[]>(CancellationToken.None);
        breeds.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("/api/v1/dogs")]
    [InlineData("/api/v1/dogs/11111111-1111-1111-1111-111111111111/photos")]
    [InlineData("/api/v1/dogs/11111111-1111-1111-1111-111111111111/health")]
    [InlineData("/api/v1/discovery?sourceDogId=11111111-1111-1111-1111-111111111111")]
    [InlineData("/api/v1/matches")]
    [InlineData("/api/v1/notifications")]
    [InlineData("/api/v1/chat/conversations")]
    [InlineData("/api/v1/meetings")]
    [InlineData("/api/v1/spaces")]
    [InlineData("/api/v1/spaces/11111111-1111-1111-1111-111111111111/availability")]
    [InlineData("/api/v1/reservations")]
    [InlineData("/api/v1/partners/reservations")]
    [InlineData("/api/v1/partners/me")]
    [InlineData("/api/v1/partners/me/spaces")]
    [InlineData("/api/v1/adoption")]
    [InlineData("/api/v1/adoption/applications")]
    [InlineData("/api/v1/adoption/11111111-1111-1111-1111-111111111111/applications")]
    [InlineData("/api/v1/adoption/applications/11111111-1111-1111-1111-111111111111/history")]
    [InlineData("/api/v1/reports")]
    [InlineData("/api/v1/moderation/reports")]
    [InlineData("/api/v1/moderation/evidence/11111111-1111-1111-1111-111111111111")]
    [InlineData("/api/v1/moderation/reports/11111111-1111-1111-1111-111111111111/evidence")]
    [InlineData("/api/v1/reservations/11111111-1111-1111-1111-111111111111/history")]
    [InlineData("/api/v1/partners/reservations/11111111-1111-1111-1111-111111111111/history")]
    public async Task OwnerDataShouldRejectAnonymousUsers(string path)
    {
        HttpResponseMessage response = await client.GetAsync(path, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("/api/v1/dogs/11111111-1111-1111-1111-111111111111/likes")]
    [InlineData("/api/v1/dogs/11111111-1111-1111-1111-111111111111/passes")]
    [InlineData("/api/v1/dogs/11111111-1111-1111-1111-111111111111/block-owner")]
    public async Task DiscoveryMutationsShouldRejectAnonymousUsers(string path)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, path) { Content = JsonContent.Create(new { sourceDogId = Guid.NewGuid() }) };
        HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MarkConversationReadShouldRejectAnonymousUsers()
    {
        using HttpRequestMessage request = new(HttpMethod.Put, "/api/v1/chat/conversations/11111111-1111-1111-1111-111111111111/read")
        {
            Content = JsonContent.Create(new { messageId = Guid.NewGuid() }),
        };

        HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
