using PetMach.Contracts.Identity;
using PetMach.Domain.SharedKernel;

namespace PetMach.Application.Identity;

public interface IIdentityService
{
    Task<Result<RegistrationResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken);
    Task<Result<TokenResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<Result<TokenResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
    Task<Result> LogoutAsync(Guid userId, LogoutRequest request, CancellationToken cancellationToken);
    Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);
    Task<Result<AccountResponse>> GetAccountAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result> AnonymizeAccountAsync(Guid userId, DeleteAccountRequest request, CancellationToken cancellationToken);
    Task<Result> SetSuspensionAsync(Guid actorUserId, Guid targetUserId, SetAccountSuspensionRequest request, CancellationToken cancellationToken);
}
