namespace PetMach.Mobile.Core.Navigation;

public interface IRootNavigationService
{
    Task ShowPublicRootAsync(CancellationToken cancellationToken);
    Task ShowAuthenticatedRootAsync(string route, CancellationToken cancellationToken);
}

public interface IAuthenticatedConnectionManager
{
    Task StopAuthenticatedConnectionsAsync(CancellationToken cancellationToken);
}
