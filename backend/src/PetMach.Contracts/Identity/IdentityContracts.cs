namespace PetMach.Contracts.Identity;

public sealed record RegisterRequest(
    string Email,
    string Password,
    DateOnly BirthDate,
    bool AcceptTerms,
    string TermsVersion,
    bool AcceptPrivacyPolicy,
    string PrivacyPolicyVersion);

public sealed record RegistrationResponse(Guid UserId, bool RequiresEmailConfirmation);

public sealed record ConfirmEmailRequest(Guid UserId, string Token);

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record LogoutRequest(string RefreshToken);

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(Guid UserId, string Token, string NewPassword);

public sealed record DeleteAccountRequest(string Password);

public sealed record SetAccountSuspensionRequest(bool Suspended);

public sealed record TokenResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc);

public sealed record AccountResponse(Guid Id, string Email, string Status, IReadOnlyCollection<string> Roles);
