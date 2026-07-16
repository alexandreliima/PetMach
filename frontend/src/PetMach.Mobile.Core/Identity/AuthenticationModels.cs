namespace PetMach.Mobile.Core.Identity;

public sealed record LoginInput(string Email, string Password);

public sealed record RegistrationInput(
    string Email,
    string Password,
    DateOnly BirthDate,
    bool AcceptTerms,
    string TermsVersion,
    bool AcceptPrivacyPolicy,
    string PrivacyPolicyVersion);

public sealed record RegistrationResult(Guid UserId, bool RequiresEmailConfirmation);

public sealed record TokenEnvelope(string AccessToken, DateTimeOffset AccessTokenExpiresAtUtc, string RefreshToken, DateTimeOffset RefreshTokenExpiresAtUtc);

public sealed record StoredSession(string AccessToken, DateTimeOffset AccessTokenExpiresAtUtc, string RefreshToken, DateTimeOffset RefreshTokenExpiresAtUtc);
