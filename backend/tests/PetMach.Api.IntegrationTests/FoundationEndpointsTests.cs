using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PetMach.Application.Identity;
using PetMach.Contracts.Identity;
using PetMach.Contracts.System;
using PetMach.Domain.SharedKernel;

namespace PetMach.Api.IntegrationTests;

public sealed class FoundationEndpointsTests : IClassFixture<PetMachApiFactory>
{
    private readonly HttpClient client;

    public FoundationEndpointsTests(PetMachApiFactory factory) => client = factory.CreateClient();

    [Fact]
    public async Task LivenessShouldBePublicAndHealthy()
    {
        HttpResponseMessage response = await client.GetAsync("/health/live", CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SystemInfoShouldReturnVersionedContract()
    {
        SystemInfoResponse? response = await client.GetFromJsonAsync<SystemInfoResponse>(
            "/api/v1/system",
            CancellationToken.None);

        response.Should().NotBeNull();
        response!.Service.Should().Be("PetMach.Api");
        response.Environment.Should().Be("Development");
    }
}

public sealed class PetMachApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IIdentityService>();
            services.AddSingleton<IIdentityService, StubIdentityService>();
        });
    }
}

internal sealed class StubIdentityService : IIdentityService
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTimeOffset Expiry = new(2026, 7, 14, 13, 0, 0, TimeSpan.Zero);

    public Task<Result<RegistrationResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(Result.Success(new RegistrationResponse(UserId, true)));
    public Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken) => Task.FromResult(Result.Success());
    public Task<Result<TokenResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(Result.Success(new TokenResponse("access", Expiry, "refresh", Expiry.AddDays(30))));
    public Task<Result<TokenResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(Result.Success(new TokenResponse("access-2", Expiry, "refresh-2", Expiry.AddDays(30))));
    public Task<Result> LogoutAsync(Guid userId, LogoutRequest request, CancellationToken cancellationToken) => Task.FromResult(Result.Success());
    public Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken) => Task.FromResult(Result.Success());
    public Task<Result<AccountResponse>> GetAccountAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult(Result.Success(new AccountResponse(UserId, "tutor@petmach.local", "Active", ["Tutor"])));
    public Task<Result> AnonymizeAccountAsync(Guid userId, DeleteAccountRequest request, CancellationToken cancellationToken) => Task.FromResult(Result.Success());
    public Task<Result> SetSuspensionAsync(Guid actorUserId, Guid targetUserId, SetAccountSuspensionRequest request, CancellationToken cancellationToken) => Task.FromResult(Result.Success());
}
