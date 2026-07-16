using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PetMach.Application.Adoption;
using PetMach.Application.Chat;
using PetMach.Application.Discovery;
using PetMach.Application.Dogs;
using PetMach.Application.Health;
using PetMach.Application.Identity;
using PetMach.Application.Matches;
using PetMach.Application.Meetings;
using PetMach.Application.Moderation;
using PetMach.Application.Notifications;
using PetMach.Application.Partners;
using PetMach.Application.Reservations;
using PetMach.Application.Tutors;
using PetMach.Domain.Identity;
using PetMach.Infrastructure.Adoption;
using PetMach.Infrastructure.Chat;
using PetMach.Infrastructure.Discovery;
using PetMach.Infrastructure.Dogs;
using PetMach.Infrastructure.Health;
using PetMach.Infrastructure.Identity;
using PetMach.Infrastructure.Matches;
using PetMach.Infrastructure.Meetings;
using PetMach.Infrastructure.Moderation;
using PetMach.Infrastructure.Notifications;
using PetMach.Infrastructure.Partners;
using PetMach.Infrastructure.Persistence;
using PetMach.Infrastructure.Reservations;
using PetMach.Infrastructure.Tutors;

namespace PetMach.Infrastructure;

public static class DependencyInjection
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=petmach;Username=petmach";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("petmach")
            ?? DefaultConnectionString;

        services.AddOptions<PetMachIdentityOptions>()
            .Bind(configuration.GetSection(PetMachIdentityOptions.SectionName))
            .Validate(options => options.AccessTokenMinutes is >= 5 and <= 60, "AccessTokenMinutes deve estar entre 5 e 60.")
            .Validate(options => options.RefreshTokenDays is >= 1 and <= 90, "RefreshTokenDays deve estar entre 1 e 90.")
            .ValidateOnStart();
        services.AddSingleton<JwtSigningKey>();
        services.AddSingleton<JwtTokenIssuer>();

        services.AddDbContext<PetMachDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(PetMachDbContext).Assembly.FullName)));

        services
            .AddIdentityCore<PetMachUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequiredLength = 12;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<PetMachDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<JwtSigningKey, IOptions<PetMachIdentityOptions>>((jwt, key, configured) =>
            {
                PetMachIdentityOptions options = configured.Value;
                jwt.MapInboundClaims = false;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = options.Issuer,
                    ValidateAudience = true,
                    ValidAudience = options.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key.Bytes),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "sub",
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                };
                jwt.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        string? subject = context.Principal?.FindFirst("sub")?.Value;
                        if (!Guid.TryParse(subject, out Guid userId))
                        {
                            context.Fail("Token sem sujeito válido.");
                            return;
                        }

                        UserManager<PetMachUser> users = context.HttpContext.RequestServices.GetRequiredService<UserManager<PetMachUser>>();
                        PetMachUser? user = await users.FindByIdAsync(userId.ToString());
                        if (user is null || user.Status != AccountStatus.Active)
                            context.Fail("Conta indisponível.");
                    },
                };
            });

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITutorProfileService, TutorProfileService>();
        services.AddScoped<IDogService, DogService>();
        services.AddScoped<IDogPhotoService, DogPhotoService>();
        services.AddScoped<IDogHealthService, DogHealthService>();
        services.AddScoped<IDiscoveryService, DiscoveryService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IMatchService, MatchService>();
        services.AddScoped<IMeetingService, MeetingService>();
        services.AddScoped<IPartnerService, PartnerService>();
        services.AddScoped<IAdoptionService, AdoptionService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IBlockService, BlockService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ITransactionalEmailSender>(provider =>
        {
            IHostEnvironment environment = provider.GetRequiredService<IHostEnvironment>();
            return environment.IsDevelopment() || environment.IsEnvironment("Testing")
                ? ActivatorUtilities.CreateInstance<DevelopmentEmailSender>(provider)
                : new UnavailableEmailSender();
        });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.AddPolicy("AdministrationAccess", policy =>
                policy.RequireRole("Administrator", "Moderator"));
            options.AddPolicy("TutorAccess", policy => policy.RequireRole(PetMachRoles.Tutor));
            options.AddPolicy("PartnerAccess", policy => policy.RequireRole(PetMachRoles.Partner));
        });

        services.AddHealthChecks()
            .AddDbContextCheck<PetMachDbContext>("postgresql", tags: ["ready"]);

        return services;
    }
}
