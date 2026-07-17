using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Core.Identity;

public sealed class AppStartupCoordinator(
    AuthenticationSession session,
    IRootNavigationService rootNavigation)
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        bool restored = await session.TryRestoreAsync(cancellationToken);
        if (restored)
        {
            await rootNavigation.ShowAuthenticatedRootAsync("//app/network", cancellationToken);
            return;
        }

        await rootNavigation.ShowPublicRootAsync(cancellationToken);
    }
}
