using Microsoft.Extensions.DependencyInjection;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Navigation;

public sealed class RootNavigationService(IServiceProvider services) : IRootNavigationService
{
    private Window? window;

    public Page CreateInitialPublicRoot() => CreatePublicRoot();

    public void AttachWindow(Window applicationWindow)
    {
        ArgumentNullException.ThrowIfNull(applicationWindow);
        if (Interlocked.CompareExchange(ref window, applicationWindow, null) is not null)
        {
            throw new InvalidOperationException("A janela raiz já foi associada ao coordenador.");
        }
    }

    public Task ShowPublicRootAsync(CancellationToken cancellationToken) =>
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            GetWindow().Page = CreatePublicRoot();
        });

    public Task ShowAuthenticatedRootAsync(string route, CancellationToken cancellationToken) =>
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            AppShell shell = services.GetRequiredService<AppShell>();
            GetWindow().Page = shell;
            await shell.GoToAsync(route);
        });

    public Window GetWindow() =>
        window ?? throw new InvalidOperationException("A janela do aplicativo ainda não foi associada.");

    private NavigationPage CreatePublicRoot()
    {
        MainPage mainPage = services.GetRequiredService<MainPage>();
        NavigationPage.SetHasNavigationBar(mainPage, false);
        return new NavigationPage(mainPage)
        {
            BarBackgroundColor = Color.FromArgb("#FFF9F3"),
            BarTextColor = Color.FromArgb("#123C38"),
        };
    }
}
