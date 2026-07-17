using System.Net.Http.Headers;
using System.Net.Http.Json;
using PetMach.Contracts.Identity;
using PetMach.Contracts.Moderation;

namespace PetMach.Admin;

public sealed class AdminApiClient(HttpClient httpClient)
{
    public async Task<TokenResponse?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/v1/auth/login", new LoginRequest(email, password), cancellationToken);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken) : null;
    }

    public Task<AccountResponse?> AccountAsync(string token, CancellationToken cancellationToken) => GetAsync<AccountResponse>("api/v1/auth/me", token, cancellationToken);
    public async Task<IReadOnlyCollection<ReportResponse>> ReportsAsync(string token, CancellationToken cancellationToken) =>
        await GetAsync<ReportResponse[]>("api/v1/moderation/reports", token, cancellationToken) ?? [];
    public async Task<IReadOnlyCollection<ReportEvidenceResponse>> EvidenceListAsync(string token, Guid reportId, CancellationToken cancellationToken) =>
        await GetAsync<ReportEvidenceResponse[]>($"api/v1/moderation/reports/{reportId}/evidence", token, cancellationToken) ?? [];

    public Task<bool> TransitionAsync(string token, Guid reportId, string transition, CancellationToken cancellationToken) =>
        PutAsync($"api/v1/moderation/reports/{reportId}/{transition}", token, null, cancellationToken);

    public Task<bool> ApplyActionAsync(string token, Guid reportId, string action, CancellationToken cancellationToken) =>
        PostAsync($"api/v1/moderation/reports/{reportId}/actions", token, JsonContent.Create(new { Action = ActionValue(action) }), cancellationToken);

    public async Task<HttpResponseMessage> EvidenceAsync(string token, Guid evidenceId, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = new(HttpMethod.Get, $"api/v1/moderation/evidence/{evidenceId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    private async Task<T?> GetAsync<T>(string path, string token, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<T>(cancellationToken) : default;
    }

    private async Task<bool> PutAsync(string path, string token, HttpContent? content, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(HttpMethod.Put, path) { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private async Task<bool> PostAsync(string path, string token, HttpContent content, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, path) { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private static int ActionValue(string action) => action switch { "SuspendUser" => 0, "SuspendDog" => 1, _ => 2 };
}
