using System.Net;
using System.Text;
using FluentAssertions;
using PetMach.Mobile.Core.Features;
using PetMach.Mobile.Core.Identity;

namespace PetMach.Mobile.Tests.Features;

public sealed class PetMachApiClientTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 17, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ProtectedFeaturesShouldRequireStoredAuthentication()
    {
        EmptyTokenStore tokens = new();
        AuthenticationSession session = new(new UnusedAuthApi(), tokens, TimeProvider.System);
        PetMachApiClient client = new(
            new HttpClient(new RejectNetworkHandler()) { BaseAddress = new Uri("http://localhost/") },
            session);

        Func<Task> action = async () =>
            _ = await client.GetDogsAsync(TestCancellation.Token);

        await action.Should().ThrowAsync<AuthenticationRequiredException>();
    }

    [Fact]
    public async Task UnauthorizedResponseShouldRefreshAndRepeatRequestOnce()
    {
        MemoryTokenStore tokens = StoreWithValidAccessToken();
        RefreshingAuthApi auth = new();
        AuthenticationSession session = new(auth, tokens, new FixedTimeProvider(Now));
        UnauthorizedThenSuccessHandler handler = new(alwaysUnauthorized: false);
        PetMachApiClient client = new(
            new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") },
            session);

        IReadOnlyCollection<DogModel> dogs =
            await client.GetDogsAsync(TestCancellation.Token);

        dogs.Should().BeEmpty();
        handler.Calls.Should().Be(2);
        handler.AccessTokens.Should().ContainInOrder("old-access", "new-access");
        auth.RefreshCalls.Should().Be(1);
        tokens.Session!.AccessToken.Should().Be("new-access");
    }

    [Fact]
    public async Task PersistentUnauthorizedResponseShouldNotBeRetriedMoreThanOnce()
    {
        MemoryTokenStore tokens = StoreWithValidAccessToken();
        RefreshingAuthApi auth = new();
        AuthenticationSession session = new(auth, tokens, new FixedTimeProvider(Now));
        UnauthorizedThenSuccessHandler handler = new(alwaysUnauthorized: true);
        PetMachApiClient client = new(
            new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") },
            session);

        Func<Task> action = async () =>
            _ = await client.GetDogsAsync(TestCancellation.Token);

        await action.Should().ThrowAsync<HttpRequestException>();
        handler.Calls.Should().Be(2);
        auth.RefreshCalls.Should().Be(1);
    }

    private static MemoryTokenStore StoreWithValidAccessToken() =>
        new()
        {
            Session = new StoredSession(
                "old-access",
                Now.AddMinutes(10),
                "old-refresh",
                Now.AddDays(1)),
        };

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class EmptyTokenStore : ITokenStore
    {
        public Task<StoredSession?> GetAsync(CancellationToken cancellationToken) =>
            Task.FromResult<StoredSession?>(null);
        public Task SaveAsync(StoredSession session, CancellationToken cancellationToken) =>
            Task.CompletedTask;
        public Task ClearAsync(CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class MemoryTokenStore : ITokenStore
    {
        public StoredSession? Session { get; set; }
        public Task<StoredSession?> GetAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Session);
        public Task SaveAsync(StoredSession session, CancellationToken cancellationToken)
        {
            Session = session;
            return Task.CompletedTask;
        }
        public Task ClearAsync(CancellationToken cancellationToken)
        {
            Session = null;
            return Task.CompletedTask;
        }
    }

    private sealed class RefreshingAuthApi : IAuthApiClient
    {
        public int RefreshCalls { get; private set; }
        public Task<RegistrationResult> RegisterAsync(RegistrationInput input, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task<TokenEnvelope> LoginAsync(LoginInput input, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task<TokenEnvelope> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
        {
            RefreshCalls++;
            return Task.FromResult(new TokenEnvelope(
                "new-access",
                Now.AddMinutes(15),
                "new-refresh",
                Now.AddDays(30)));
        }
        public Task LogoutAsync(string accessToken, string refreshToken, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class UnusedAuthApi : IAuthApiClient
    {
        public Task<RegistrationResult> RegisterAsync(RegistrationInput input, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task<TokenEnvelope> LoginAsync(LoginInput input, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task<TokenEnvelope> RefreshAsync(string refreshToken, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task LogoutAsync(string accessToken, string refreshToken, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class RejectNetworkHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("A rede não deveria ser acessada sem autenticação.");
    }

    private sealed class UnauthorizedThenSuccessHandler(bool alwaysUnauthorized) : HttpMessageHandler
    {
        public int Calls { get; private set; }
        public List<string?> AccessTokens { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Calls++;
            AccessTokens.Add(request.Headers.Authorization?.Parameter);
            if (alwaysUnauthorized || Calls == 1)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json"),
            });
        }
    }
}
