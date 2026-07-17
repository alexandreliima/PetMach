using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using PetMach.Admin;
using PetMach.Admin.Components;
using PetMach.Contracts.Identity;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options => { options.IncludeScopes = true; options.SingleLine = true; options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ "; options.UseUtcTimestamp = true; });
builder.AddServiceDefaults();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/login"; options.Cookie.HttpOnly = true; options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; options.SlidingExpiration = false;
});
builder.Services.AddAuthorizationBuilder().AddPolicy("AdministrationAccess", policy => policy.RequireRole("Administrator", "Moderator"));
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpClient<AdminApiClient>(client => client.BaseAddress = new Uri(builder.Configuration["PetMachApi:BaseUrl"] ?? "http://localhost:5049/"));

WebApplication app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error", createScopeForErrors: true); app.UseHsts(); }
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection(); app.UseAuthentication(); app.UseAuthorization(); app.UseAntiforgery(); app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapPost("/admin/login", async (HttpContext context, AdminApiClient api, IAntiforgery antiforgery, CancellationToken ct) =>
{
    await antiforgery.ValidateRequestAsync(context); IFormCollection form = await context.Request.ReadFormAsync(ct);
    TokenResponse? tokens = await api.LoginAsync(form["email"].ToString(), form["password"].ToString(), ct);
    if (tokens is null) return Results.Redirect("/login?error=1");
    AccountResponse? account = await api.AccountAsync(tokens.AccessToken, ct);
    if (account is null || !account.Roles.Any(x => x is "Administrator" or "Moderator")) return Results.Redirect("/login?error=role");
    List<Claim> claims = [new(ClaimTypes.NameIdentifier, account.Id.ToString()), new(ClaimTypes.Email, account.Email), new("access_token", tokens.AccessToken)];
    claims.AddRange(account.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
    await context.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)), new AuthenticationProperties { ExpiresUtc = tokens.AccessTokenExpiresAtUtc });
    return Results.Redirect("/");
});
app.MapPost("/admin/logout", async (HttpContext context, IAntiforgery antiforgery) => { await antiforgery.ValidateRequestAsync(context); await context.SignOutAsync(); return Results.Redirect("/login"); }).RequireAuthorization();
app.MapPost("/admin/reports/{reportId:guid}/{transition}", async (Guid reportId, string transition, HttpContext context, AdminApiClient api, IAntiforgery antiforgery, CancellationToken ct) =>
{ await antiforgery.ValidateRequestAsync(context); await api.TransitionAsync(Token(context), reportId, transition, ct); return Results.Redirect("/"); }).RequireAuthorization("AdministrationAccess");
app.MapPost("/admin/reports/{reportId:guid}/actions/{action}", async (Guid reportId, string action, HttpContext context, AdminApiClient api, IAntiforgery antiforgery, CancellationToken ct) =>
{ await antiforgery.ValidateRequestAsync(context); await api.ApplyActionAsync(Token(context), reportId, action, ct); return Results.Redirect("/"); }).RequireAuthorization("AdministrationAccess");
app.MapGet("/admin/evidence/{evidenceId:guid}", async (Guid evidenceId, HttpContext context, AdminApiClient api, CancellationToken ct) =>
{
    using HttpResponseMessage response = await api.EvidenceAsync(Token(context), evidenceId, ct);
    return response.IsSuccessStatusCode ? Results.File(await response.Content.ReadAsByteArrayAsync(ct), response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream") : Results.NotFound();
}).RequireAuthorization("AdministrationAccess");
app.MapDefaultEndpoints(); app.Run();

static string Token(HttpContext context) => context.User.FindFirstValue("access_token") ?? string.Empty;
