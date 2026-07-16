using FluentValidation;
using PetMach.Contracts.Identity;

namespace PetMach.Application.Identity;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.Password).NotEmpty().MinimumLength(12).MaximumLength(128);
        RuleFor(request => request.BirthDate).NotEmpty();
        RuleFor(request => request.AcceptTerms).Equal(true);
        RuleFor(request => request.TermsVersion).NotEmpty().MaximumLength(32);
        RuleFor(request => request.AcceptPrivacyPolicy).Equal(true);
        RuleFor(request => request.PrivacyPolicyVersion).NotEmpty().MaximumLength(32);
    }
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress();
        RuleFor(request => request.Password).NotEmpty();
    }
}

public sealed class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator() => RuleFor(request => request.RefreshToken).NotEmpty().MaximumLength(512);
}

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(request => request.UserId).NotEmpty();
        RuleFor(request => request.Token).NotEmpty().MaximumLength(2048);
        RuleFor(request => request.NewPassword).NotEmpty().MinimumLength(12).MaximumLength(128);
    }
}
