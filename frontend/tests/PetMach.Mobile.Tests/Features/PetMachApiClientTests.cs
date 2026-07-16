using FluentAssertions;
using PetMach.Mobile.Core.Features;
using PetMach.Mobile.Core.Identity;

namespace PetMach.Mobile.Tests.Features;

public sealed class PetMachApiClientTests
{
    [Fact]
    public async Task ProtectedFeaturesShouldRequireStoredAuthentication()
    {
        EmptyTokenStore tokens = new();
        AuthenticationSession session = new(new UnusedAuthApi(), tokens, TimeProvider.System);
        PetMachApiClient client = new(new HttpClient(new RejectNetworkHandler()) { BaseAddress = new Uri("http://localhost/") }, session);

        Func<Task> action = async () => _ = await client.GetDogsAsync(CancellationToken.None);

        await action.Should().ThrowAsync<AuthenticationRequiredException>();
    }

    private sealed class EmptyTokenStore : ITokenStore
    {
        public Task<StoredSession?> GetAsync(CancellationToken cancellationToken) => Task.FromResult<StoredSession?>(null);
        public Task SaveAsync(StoredSession session, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task ClearAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class UnusedAuthApi : IAuthApiClient
    {
        public Task<RegistrationResult> RegisterAsync(RegistrationInput input, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<TokenEnvelope> LoginAsync(LoginInput input, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<TokenEnvelope> RefreshAsync(string refreshToken, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task LogoutAsync(string accessToken, string refreshToken, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class RejectNetworkHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("A rede não deveria ser acessada sem autenticação.");
    }
}
