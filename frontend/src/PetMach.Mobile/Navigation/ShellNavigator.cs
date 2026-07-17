using Microsoft.Extensions.DependencyInjection;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Navigation;

public sealed class ShellNavigator(
    IServiceProvider services,
    RootNavigationService rootNavigation) : IMobileNavigator
{
    public async Task GoToAsync(string route)
    {
        if (route.StartsWith("//app/", StringComparison.Ordinal))
        {
            using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(15));
            await rootNavigation.ShowAuthenticatedRootAsync(route, timeout.Token);
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            Window window = rootNavigation.GetWindow();
            if (window.Page is Shell shell)
            {
                await shell.GoToAsync(route);
                return;
            }

            Page page = CreatePublicPage(route);
            if (window.Page is not NavigationPage navigation)
            {
                throw new InvalidOperationException("A raiz pública não está disponível.");
            }

            await navigation.PushAsync(page);
        });
    }

    private Page CreatePublicPage(string route)
    {
        string path = route.Split('?', 2)[0];
        Page page = path switch
        {
            "login" => services.GetRequiredService<LoginPage>(),
            "register" => services.GetRequiredService<RegisterPage>(),
            "about" => services.GetRequiredService<AboutPage>(),
            "onboarding" => services.GetRequiredService<OnboardingPage>(),
            _ => throw new InvalidOperationException($"Rota pública desconhecida: {path}"),
        };

        if (page is IQueryAttributable queryPage && route.Contains('?', StringComparison.Ordinal))
        {
            queryPage.ApplyQueryAttributes(ParseQuery(route));
        }

        return page;
    }

    private static Dictionary<string, object> ParseQuery(string route)
    {
        Dictionary<string, object> values = new(StringComparer.OrdinalIgnoreCase);
        string query = route.Split('?', 2)[1];
        foreach (string pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] parts = pair.Split('=', 2);
            string key = Uri.UnescapeDataString(parts[0]);
            string value = parts.Length == 2 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
            values[key] = value;
        }

        return values;
    }
}
