using PetMach.Mobile.Core.Identity;
using PetMach.Mobile.Navigation;

namespace PetMach.Mobile;

public partial class App : Application
{
    private readonly RootNavigationService rootNavigation;
    private readonly AppStartupCoordinator startup;
    private readonly SessionLifecycleCoordinator sessionLifecycle;

    public App(
        RootNavigationService rootNavigation,
        AppStartupCoordinator startup,
        SessionLifecycleCoordinator sessionLifecycle)
    {
        InitializeComponent();
        this.rootNavigation = rootNavigation;
        this.startup = startup;
        this.sessionLifecycle = sessionLifecycle;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        CancellationTokenSource lifetime = new();
        Window window = new(rootNavigation.CreateInitialPublicRoot());
        window.Destroying += (_, _) => lifetime.Cancel();
        rootNavigation.AttachWindow(window);
        sessionLifecycle.Start();
        _ = InitializeAsync(lifetime.Token);
        return window;
    }

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        try
        {
            await startup.InitializeAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }
}
