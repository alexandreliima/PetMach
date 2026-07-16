using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PetMach.Contracts.Identity;

namespace PetMach.Infrastructure.Identity;

internal sealed class JwtTokenIssuer(
    JwtSigningKey signingKey,
    IOptions<PetMachIdentityOptions> options,
    TimeProvider timeProvider)
{
    private readonly PetMachIdentityOptions settings = options.Value;

    public TokenResponse Issue(PetMachUser user, IReadOnlyCollection<string> roles, string refreshToken, DateTimeOffset refreshExpiresAtUtc)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        DateTimeOffset expires = now.AddMinutes(settings.AccessTokenMinutes);
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        ];
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        SigningCredentials credentials = new(
            new SymmetricSecurityKey(signingKey.Bytes),
            SecurityAlgorithms.HmacSha256);
        JwtSecurityToken token = new(
            settings.Issuer,
            settings.Audience,
            claims,
            now.UtcDateTime,
            expires.UtcDateTime,
            credentials);

        return new TokenResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expires,
            refreshToken,
            refreshExpiresAtUtc);
    }
}
