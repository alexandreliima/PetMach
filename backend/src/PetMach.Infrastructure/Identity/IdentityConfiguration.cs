using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace PetMach.Infrastructure.Identity;

public sealed class PetMachIdentityOptions
{
    public const string SectionName = "Identity";
    public string Issuer { get; set; } = "PetMach.Api";
    public string Audience { get; set; } = "PetMach.Mobile";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 30;
    public int MinimumTutorAge { get; set; } = 18;
    public string CurrentTermsVersion { get; set; } = "2026-07-14";
    public string CurrentPrivacyVersion { get; set; } = "2026-07-14";
}

public static class PetMachRoles
{
    public const string Tutor = "Tutor";
    public const string Partner = "Partner";
    public const string Moderator = "Moderator";
    public const string Administrator = "Administrator";
}

internal sealed class JwtSigningKey
{
    public JwtSigningKey(IConfiguration configuration, IHostEnvironment environment)
    {
        string? configuredKey = configuration["Identity:SigningKey"];
        if (!string.IsNullOrWhiteSpace(configuredKey))
        {
            Bytes = Encoding.UTF8.GetBytes(configuredKey);
        }
        else if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            Bytes = RandomNumberGenerator.GetBytes(64);
        }
        else
        {
            throw new InvalidOperationException("Identity:SigningKey deve ser configurada fora do repositório.");
        }

        if (Bytes.Length < 32)
        {
            throw new InvalidOperationException("Identity:SigningKey deve possuir ao menos 32 bytes.");
        }
    }

    public byte[] Bytes { get; }
}
