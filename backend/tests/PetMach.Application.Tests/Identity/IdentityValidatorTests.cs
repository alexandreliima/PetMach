using FluentAssertions;
using PetMach.Application.Identity;
using PetMach.Contracts.Identity;

namespace PetMach.Application.Tests.Identity;

public sealed class IdentityValidatorTests
{
    [Fact]
    public void RegisterShouldRejectWeakOrUnconsentedRequest()
    {
        RegisterRequest request = new("invalid", "short", new DateOnly(2000, 1, 1), false, string.Empty, false, string.Empty);

        new RegisterRequestValidator().Validate(request).IsValid.Should().BeFalse();
    }

    [Fact]
    public void RegisterShouldAcceptStructurallyValidRequest()
    {
        RegisterRequest request = new("tutor@petmach.local", "UmaSenhaForte!123", new DateOnly(1990, 1, 1), true, "2026-07-14", true, "2026-07-14");

        new RegisterRequestValidator().Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void RefreshShouldRejectEmptyToken()
    {
        new RefreshTokenRequestValidator().Validate(new RefreshTokenRequest(string.Empty)).IsValid.Should().BeFalse();
    }
}
