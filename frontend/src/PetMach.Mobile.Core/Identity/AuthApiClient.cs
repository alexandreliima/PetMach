using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PetMach.Mobile.Core.Identity;

public sealed class AuthApiClient(HttpClient httpClient) : IAuthApiClient
{
    public async Task<RegistrationResult> RegisterAsync(RegistrationInput input, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/v1/auth/register", input, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            string message = "Não foi possível autenticar.";
            string? code = null;
            try
            {
                using JsonDocument problem = await JsonDocument.ParseAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    cancellationToken: cancellationToken);
                if (problem.RootElement.TryGetProperty("title", out JsonElement title)) message = title.GetString() ?? message;
                if (problem.RootElement.TryGetProperty("code", out JsonElement errorCode)) code = errorCode.GetString();
            }
            catch (JsonException) { }
            throw new AuthenticationApiException(code, message, response.StatusCode);
        }
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
        if (!response.IsSuccessStatusCode)
        {
            (string? code, string message) = await ReadProblemAsync(response, cancellationToken);
            throw new AuthenticationApiException(code, message, response.StatusCode);
        }
    }

    private static async Task<TokenEnvelope> ReadTokensAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            (string? code, string message) = await ReadProblemAsync(response, cancellationToken);
            throw new AuthenticationApiException(code, message, response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<TokenEnvelope>(cancellationToken)
            ?? throw new InvalidOperationException("A API retornou uma sessão vazia.");
    }

    private static async Task<(string? Code, string Message)> ReadProblemAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string message = response.StatusCode == System.Net.HttpStatusCode.Unauthorized
            ? "E-mail ou senha inválidos."
            : "Não foi possível autenticar.";
        string? code = null;

        try
        {
            using JsonDocument problem = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken);
            if (problem.RootElement.TryGetProperty("title", out JsonElement title))
            {
                message = title.GetString() ?? message;
            }

            if (problem.RootElement.TryGetProperty("code", out JsonElement errorCode))
            {
                code = errorCode.GetString();
            }
        }
        catch (JsonException)
        {
        }

        return (code, message);
    }
}

public sealed class AuthenticationApiException(string? code, string message, System.Net.HttpStatusCode statusCode)
    : HttpRequestException(message, null, statusCode)
{
    public string? Code { get; } = code;
}
