namespace PetMach.Mobile.Core.Identity;

public interface IAuthApiClient
{
    Task<RegistrationResult> RegisterAsync(RegistrationInput input, CancellationToken cancellationToken);
    Task<TokenEnvelope> LoginAsync(LoginInput input, CancellationToken cancellationToken);
    Task<TokenEnvelope> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
    Task LogoutAsync(string accessToken, string refreshToken, CancellationToken cancellationToken);
}
