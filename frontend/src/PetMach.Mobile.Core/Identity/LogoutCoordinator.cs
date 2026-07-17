using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Core.Identity;

public interface ILogoutCoordinator
{
    Task LogoutAsync(CancellationToken cancellationToken);
}

public sealed class LogoutCoordinator(
    AuthenticationSession session,
    IAuthenticatedConnectionManager connections,
    IRootNavigationService rootNavigation) : ILogoutCoordinator
{
    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        try
        {
            await session.LogoutAsync(cancellationToken);
        }
        catch (HttpRequestException)
        {
        }
        finally
        {
            using CancellationTokenSource cleanupTimeout = new(TimeSpan.FromSeconds(30));
            await connections.StopAuthenticatedConnectionsAsync(cleanupTimeout.Token);
            await rootNavigation.ShowPublicRootAsync(cleanupTimeout.Token);
        }
    }
}
