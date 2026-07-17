using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Core.Identity;

public sealed class SessionLifecycleCoordinator
{
    private readonly AuthenticationSession session;
    private readonly IAuthenticatedConnectionManager connections;
    private readonly IRootNavigationService rootNavigation;
    private int started;

    public SessionLifecycleCoordinator(
        AuthenticationSession session,
        IAuthenticatedConnectionManager connections,
        IRootNavigationService rootNavigation)
    {
        this.session = session;
        this.connections = connections;
        this.rootNavigation = rootNavigation;
    }

    public void Start()
    {
        if (Interlocked.Exchange(ref started, 1) == 0)
        {
            session.Invalidated += HandleInvalidatedAsync;
        }
    }

    private async Task HandleInvalidatedAsync(CancellationToken cancellationToken)
    {
        await connections.StopAuthenticatedConnectionsAsync(cancellationToken);
        await rootNavigation.ShowPublicRootAsync(cancellationToken);
    }
}
