using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PetMach.Contracts.Identity;

namespace PetMach.Api.IntegrationTests;

public sealed class AuthenticationEndpointsTests : IClassFixture<PetMachApiFactory>
{
    private readonly HttpClient client;

    public AuthenticationEndpointsTests(PetMachApiFactory factory) => client = factory.CreateClient();

    [Fact]
    public async Task LoginShouldBePublicAndReturnTokenContract()
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("tutor@petmach.local", "UmaSenhaForte!123"),
            CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        TokenResponse? tokens = await response.Content.ReadFromJsonAsync<TokenResponse>(CancellationToken.None);
        tokens!.AccessToken.Should().Be("access");
    }

    [Fact]
    public async Task RegistrationShouldReturnCreatedContract()
    {
        RegisterRequest request = new("tutor@petmach.local", "UmaSenhaForte!123", new DateOnly(1990, 1, 1), true, "2026-07-14", true, "2026-07-14");

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/auth/register", request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        RegistrationResponse? registration = await response.Content.ReadFromJsonAsync<RegistrationResponse>(CancellationToken.None);
        registration!.RequiresEmailConfirmation.Should().BeTrue();
    }

    [Theory]
    [InlineData("/api/v1/auth/me", "GET")]
    [InlineData("/api/v1/auth/logout", "POST")]
    public async Task ProtectedEndpointsShouldRejectAnonymousUser(string path, string method)
    {
        using HttpRequestMessage request = new(new HttpMethod(method), path);
        if (method == "POST") request.Content = JsonContent.Create(new LogoutRequest("refresh"));

        HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PasswordRecoveryShouldNotRevealIfEmailExists()
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/v1/auth/forgot-password",
            new ForgotPasswordRequest("unknown@petmach.local"),
            CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    [Fact]
    public async Task SuspensionShouldRequireAdministrationPolicy()
    {
        using HttpRequestMessage request = new(
            HttpMethod.Patch,
            "/api/v1/administration/users/11111111-1111-1111-1111-111111111111/suspension")
        {
            Content = JsonContent.Create(new SetAccountSuspensionRequest(true)),
        };

        HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TutorProfileShouldRejectAnonymousUser()
    {
        HttpResponseMessage response = await client.GetAsync("/api/v1/tutors/me", CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
