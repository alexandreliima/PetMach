using PetMach.Domain.SharedKernel;

namespace PetMach.Domain.Identity;

public static class IdentityErrors
{
    public static readonly DomainError InvalidCredentials = new("identity.invalid_credentials", "E-mail ou senha inválidos.");
    public static readonly DomainError EmailAlreadyRegistered = new("identity.email_already_registered", "Este e-mail já está cadastrado.");
    public static readonly DomainError EmailNotConfirmed = new("identity.email_not_confirmed", "Confirme o e-mail antes de entrar.");
    public static readonly DomainError AccountUnavailable = new("identity.account_unavailable", "A conta não está disponível.");
    public static readonly DomainError InvalidToken = new("identity.invalid_token", "O token é inválido ou expirou.");
    public static readonly DomainError RefreshTokenReuse = new("identity.refresh_token_reuse", "A sessão foi revogada por segurança.");
    public static readonly DomainError TermsRequired = new("identity.terms_required", "É necessário aceitar os termos e a política de privacidade vigentes.");
    public static readonly DomainError MinimumAge = new("identity.minimum_age", "A idade mínima provisória é 18 anos.");
    public static readonly DomainError InvalidRequest = new("identity.invalid_request", "Os dados informados são inválidos.");
}
