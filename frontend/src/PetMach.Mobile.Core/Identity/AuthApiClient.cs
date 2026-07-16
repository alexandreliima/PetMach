using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PetMach.Mobile.Core.Identity;

public sealed class AuthApiClient(HttpClient httpClient) : IAuthApiClient
{
    public async Task<RegistrationResult> RegisterAsync(RegistrationInput input, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/v1/auth/register", input, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RegistrationResult>(cancellationToken)
            ?? throw new InvalidOperationException("A API retornou um cadastro vazio.");
    }

    public async Task<TokenEnvelope> LoginAsync(LoginInput input, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/v1/auth/login", input, cancellationToken);
        return await ReadTokensAsync(response, cancellationToken);
    }

    public async Task<TokenEnvelope> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/v1/auth/refresh", new { refreshToken }, cancellationToken);
        return await ReadTokensAsync(response, cancellationToken);
    }

    public async Task LogoutAsync(string accessToken, string refreshToken, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, "api/v1/auth/logout") { Content = JsonContent.Create(new { refreshToken }) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static async Task<TokenEnvelope> ReadTokensAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TokenEnvelope>(cancellationToken)
            ?? throw new InvalidOperationException("A API retornou uma sessão vazia.");
    }
}
