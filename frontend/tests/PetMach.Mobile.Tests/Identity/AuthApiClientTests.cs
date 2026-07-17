using System.Net;
using System.Text;
using FluentAssertions;
using PetMach.Mobile.Core.Identity;

namespace PetMach.Mobile.Tests.Identity;

public sealed class AuthApiClientTests
{
    [Fact]
    public async Task InvalidCredentialsShouldNotBeReportedAsConnectionFailure()
    {
        const string problem = """
            {"title":"E-mail ou senha inválidos.","code":"identity.invalid_credentials"}
            """;
        HttpClient httpClient = new(new StubHandler(
            new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent(problem, Encoding.UTF8, "application/problem+json"),
            }))
        {
            BaseAddress = new Uri("http://localhost/"),
        };
        AuthApiClient client = new(httpClient);

        Func<Task> action = async () =>
            _ = await client.LoginAsync(new LoginInput("teste@gmail.com", "senha-incorreta"), TestCancellation.Token);

        AuthenticationApiException exception = (await action.Should()
            .ThrowAsync<AuthenticationApiException>())
            .Which;
        exception.Message.Should().Be("E-mail ou senha inválidos.");
        exception.Code.Should().Be("identity.invalid_credentials");
        exception.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshEndpointShouldNotRetryItself()
    {
        CountingHandler handler = new(
            new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent(
                    """{"title":"Refresh inválido.","code":"identity.refresh_invalid"}""",
                    Encoding.UTF8,
                    "application/problem+json"),
            });
        AuthApiClient client = new(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/"),
        });

        Func<Task> action = async () =>
            _ = await client.RefreshAsync("refresh", TestCancellation.Token);

        await action.Should().ThrowAsync<AuthenticationApiException>();
        handler.Calls.Should().Be(1);
        handler.LastPath.Should().Be("/api/v1/auth/refresh");
    }

    private sealed class StubHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(response);
    }

    private sealed class CountingHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        public int Calls { get; private set; }
        public string? LastPath { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Calls++;
            LastPath = request.RequestUri?.AbsolutePath;
            return Task.FromResult(response);
        }
    }
}
