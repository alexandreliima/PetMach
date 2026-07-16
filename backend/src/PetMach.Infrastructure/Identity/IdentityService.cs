using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PetMach.Application.Identity;
using PetMach.Contracts.Identity;
using PetMach.Domain.Identity;
using PetMach.Domain.SharedKernel;
using PetMach.Infrastructure.Persistence;

namespace PetMach.Infrastructure.Identity;

internal sealed class IdentityService(
    UserManager<PetMachUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    SignInManager<PetMachUser> signInManager,
    PetMachDbContext dbContext,
    JwtTokenIssuer tokenIssuer,
    ITransactionalEmailSender emailSender,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator,
    IValidator<RefreshTokenRequest> refreshValidator,
    IValidator<ResetPasswordRequest> resetPasswordValidator,
    IOptions<PetMachIdentityOptions> options,
    TimeProvider timeProvider) : IIdentityService
{
    private readonly PetMachIdentityOptions settings = options.Value;

    public async Task<Result<RegistrationResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!(await registerValidator.ValidateAsync(request, cancellationToken)).IsValid)
            return Result.Failure<RegistrationResponse>(IdentityErrors.InvalidRequest);
        if (!request.AcceptTerms || !request.AcceptPrivacyPolicy ||
            request.TermsVersion != settings.CurrentTermsVersion ||
            request.PrivacyPolicyVersion != settings.CurrentPrivacyVersion)
            return Result.Failure<RegistrationResponse>(IdentityErrors.TermsRequired);
        if (!HasMinimumAge(request.BirthDate, timeProvider.GetUtcNow(), settings.MinimumTutorAge))
            return Result.Failure<RegistrationResponse>(IdentityErrors.MinimumAge);

        PetMachUser? existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            return Result.Failure<RegistrationResponse>(IdentityErrors.EmailAlreadyRegistered);

        DateTimeOffset now = timeProvider.GetUtcNow();
        PetMachUser user = new()
        {
            Id = Guid.NewGuid(),
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            BirthDate = request.BirthDate,
            CreatedAtUtc = now,
            Status = AccountStatus.PendingConfirmation,
        };
        IdentityResult created = await userManager.CreateAsync(user, request.Password);
        if (!created.Succeeded)
            return Result.Failure<RegistrationResponse>(IdentityErrors.InvalidRequest);

        if (!await roleManager.RoleExistsAsync(PetMachRoles.Tutor))
            await roleManager.CreateAsync(new IdentityRole<Guid>(PetMachRoles.Tutor));
        await userManager.AddToRoleAsync(user, PetMachRoles.Tutor);

        dbContext.ConsentRecords.Add(new ConsentRecord(user.Id, request.TermsVersion, request.PrivacyPolicyVersion, now));
        await dbContext.SaveChangesAsync(cancellationToken);

        string token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        await emailSender.SendEmailConfirmationAsync(user.Email!, user.Id, token, cancellationToken);
        return Result.Success(new RegistrationResponse(user.Id, true));
    }

    public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        PetMachUser? user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null || user.Status is AccountStatus.Anonymized or AccountStatus.Suspended)
            return Result.Failure(IdentityErrors.InvalidToken);
        IdentityResult confirmed = await userManager.ConfirmEmailAsync(user, request.Token);
        if (!confirmed.Succeeded) return Result.Failure(IdentityErrors.InvalidToken);
        user.Status = AccountStatus.Active;
        await userManager.UpdateAsync(user);
        return Result.Success();
    }

    public async Task<Result<TokenResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (!(await loginValidator.ValidateAsync(request, cancellationToken)).IsValid)
            return Result.Failure<TokenResponse>(IdentityErrors.InvalidCredentials);
        PetMachUser? user = await userManager.FindByEmailAsync(request.Email);
        if (user is null) return Result.Failure<TokenResponse>(IdentityErrors.InvalidCredentials);
        Microsoft.AspNetCore.Identity.SignInResult checkedPassword = await signInManager.CheckPasswordSignInAsync(user, request.Password, true);
        if (!checkedPassword.Succeeded) return Result.Failure<TokenResponse>(IdentityErrors.InvalidCredentials);
        if (!user.EmailConfirmed || user.Status == AccountStatus.PendingConfirmation)
            return Result.Failure<TokenResponse>(IdentityErrors.EmailNotConfirmed);
        if (user.Status != AccountStatus.Active)
            return Result.Failure<TokenResponse>(IdentityErrors.AccountUnavailable);
        return await IssueSessionAsync(user, Guid.NewGuid(), cancellationToken);
    }

    public async Task<Result<TokenResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (!(await refreshValidator.ValidateAsync(request, cancellationToken)).IsValid)
            return Result.Failure<TokenResponse>(IdentityErrors.InvalidToken);
        string hash = HashToken(request.RefreshToken);
        RefreshToken? current = await dbContext.RefreshTokens.SingleOrDefaultAsync(token => token.TokenHash == hash, cancellationToken);
        if (current is null) return Result.Failure<TokenResponse>(IdentityErrors.InvalidToken);
        DateTimeOffset now = timeProvider.GetUtcNow();
        if (current.UsedAtUtc is not null)
        {
            await RevokeFamilyAsync(current.FamilyId, now, cancellationToken);
            return Result.Failure<TokenResponse>(IdentityErrors.RefreshTokenReuse);
        }
        if (!current.IsActive(now)) return Result.Failure<TokenResponse>(IdentityErrors.InvalidToken);
        PetMachUser? user = await userManager.FindByIdAsync(current.UserId.ToString());
        if (user is null || user.Status != AccountStatus.Active)
            return Result.Failure<TokenResponse>(IdentityErrors.AccountUnavailable);

        (string plain, RefreshToken replacement) = CreateRefreshToken(user.Id, current.FamilyId, now);
        Result consumed = current.Consume(replacement.Id, now);
        if (consumed.IsFailure) return Result.Failure<TokenResponse>(consumed.Error);
        dbContext.RefreshTokens.Add(replacement);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            dbContext.ChangeTracker.Clear();
            await RevokeFamilyAsync(current.FamilyId, now, cancellationToken);
            return Result.Failure<TokenResponse>(IdentityErrors.RefreshTokenReuse);
        }
        IReadOnlyCollection<string> roles = (await userManager.GetRolesAsync(user)).ToArray();
        return Result.Success(tokenIssuer.Issue(user, roles, plain, replacement.ExpiresAtUtc));
    }

    public async Task<Result> LogoutAsync(Guid userId, LogoutRequest request, CancellationToken cancellationToken)
    {
        string hash = HashToken(request.RefreshToken);
        RefreshToken? token = await dbContext.RefreshTokens.SingleOrDefaultAsync(item => item.TokenHash == hash && item.UserId == userId, cancellationToken);
        if (token is null) return Result.Success();
        token.Revoke(timeProvider.GetUtcNow());
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        PetMachUser? user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || user.Status != AccountStatus.Active || !user.EmailConfirmed) return;
        string token = await userManager.GeneratePasswordResetTokenAsync(user);
        await emailSender.SendPasswordResetAsync(user.Email!, user.Id, token, cancellationToken);
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        if (!(await resetPasswordValidator.ValidateAsync(request, cancellationToken)).IsValid)
            return Result.Failure(IdentityErrors.InvalidToken);
        PetMachUser? user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null || user.Status != AccountStatus.Active) return Result.Failure(IdentityErrors.InvalidToken);
        IdentityResult reset = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!reset.Succeeded) return Result.Failure(IdentityErrors.InvalidToken);
        await RevokeUserSessionsAsync(user.Id, timeProvider.GetUtcNow(), cancellationToken);
        return Result.Success();
    }

    public async Task<Result<AccountResponse>> GetAccountAsync(Guid userId, CancellationToken cancellationToken)
    {
        PetMachUser? user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.Status == AccountStatus.Anonymized)
            return Result.Failure<AccountResponse>(IdentityErrors.AccountUnavailable);
        IReadOnlyCollection<string> roles = (await userManager.GetRolesAsync(user)).ToArray();
        return Result.Success(new AccountResponse(user.Id, user.Email!, user.Status.ToString(), roles));
    }

    public async Task<Result> AnonymizeAccountAsync(Guid userId, DeleteAccountRequest request, CancellationToken cancellationToken)
    {
        PetMachUser? user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result.Failure(IdentityErrors.AccountUnavailable);
        if (!await userManager.CheckPasswordAsync(user, request.Password))
            return Result.Failure(IdentityErrors.InvalidCredentials);
        DateTimeOffset now = timeProvider.GetUtcNow();
        await RevokeUserSessionsAsync(user.Id, now, cancellationToken);
        string anonymousEmail = $"deleted-{user.Id:N}@invalid.petmach";
        user.Email = anonymousEmail;
        user.NormalizedEmail = userManager.NormalizeEmail(anonymousEmail);
        user.UserName = anonymousEmail;
        user.NormalizedUserName = userManager.NormalizeName(anonymousEmail);
        user.PhoneNumber = null;
        user.PasswordHash = null;
        user.Status = AccountStatus.Anonymized;
        user.AnonymizedAtUtc = now;
        await userManager.UpdateSecurityStampAsync(user);
        IdentityResult updated = await userManager.UpdateAsync(user);
        if (!updated.Succeeded) return Result.Failure(IdentityErrors.AccountUnavailable);
        dbContext.IdentityAuditLogs.Add(new IdentityAuditLog(user.Id, "identity.account_anonymized", user.Id, now, "success"));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> SetSuspensionAsync(Guid actorUserId, Guid targetUserId, SetAccountSuspensionRequest request, CancellationToken cancellationToken)
    {
        PetMachUser? user = await userManager.FindByIdAsync(targetUserId.ToString());
        if (user is null || user.Status == AccountStatus.Anonymized)
            return Result.Failure(IdentityErrors.AccountUnavailable);
        DateTimeOffset now = timeProvider.GetUtcNow();
        user.Status = request.Suspended ? AccountStatus.Suspended : AccountStatus.Active;
        user.SuspendedAtUtc = request.Suspended ? now : null;
        await userManager.UpdateSecurityStampAsync(user);
        IdentityResult updated = await userManager.UpdateAsync(user);
        if (!updated.Succeeded) return Result.Failure(IdentityErrors.AccountUnavailable);
        if (request.Suspended) await RevokeUserSessionsAsync(user.Id, now, cancellationToken);
        dbContext.IdentityAuditLogs.Add(new IdentityAuditLog(
            actorUserId,
            request.Suspended ? "identity.account_suspended" : "identity.account_reactivated",
            targetUserId,
            now,
            "success"));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result<TokenResponse>> IssueSessionAsync(PetMachUser user, Guid familyId, CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        (string plain, RefreshToken refresh) = CreateRefreshToken(user.Id, familyId, now);
        dbContext.RefreshTokens.Add(refresh);
        await dbContext.SaveChangesAsync(cancellationToken);
        IReadOnlyCollection<string> roles = (await userManager.GetRolesAsync(user)).ToArray();
        return Result.Success(tokenIssuer.Issue(user, roles, plain, refresh.ExpiresAtUtc));
    }

    private (string Plain, RefreshToken Entity) CreateRefreshToken(Guid userId, Guid familyId, DateTimeOffset now)
    {
        string plain = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        RefreshToken entity = RefreshToken.Issue(userId, familyId, HashToken(plain), now, now.AddDays(settings.RefreshTokenDays)).Value;
        return (plain, entity);
    }

    private async Task RevokeFamilyAsync(Guid familyId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        List<RefreshToken> tokens = await dbContext.RefreshTokens.Where(token => token.FamilyId == familyId).ToListAsync(cancellationToken);
        tokens.ForEach(token => token.Revoke(now));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task RevokeUserSessionsAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        List<RefreshToken> tokens = await dbContext.RefreshTokens.Where(token => token.UserId == userId).ToListAsync(cancellationToken);
        tokens.ForEach(token => token.Revoke(now));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static bool HasMinimumAge(DateOnly birthDate, DateTimeOffset now, int minimumAge)
    {
        DateOnly today = DateOnly.FromDateTime(now.UtcDateTime);
        DateOnly threshold = today.AddYears(-minimumAge);
        return birthDate <= threshold;
    }
}
