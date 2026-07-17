using Microsoft.Extensions.DependencyInjection;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Navigation;

public sealed class ShellNavigator(IServiceProvider services) : IMobileNavigator
{
    public async Task GoToAsync(string route)
    {
        IReadOnlyList<Window>? windows = Application.Current?.Windows;
        Window window = windows is { Count: > 0 }
            ? windows[0]
            : throw new InvalidOperationException("A janela do aplicativo ainda não está disponível.");

        if (route.StartsWith("//app/", StringComparison.Ordinal))
        {
            AppShell shell = services.GetRequiredService<AppShell>();
            window.Page = shell;
            await shell.GoToAsync(route);
            return;
        }

        if (window.Page is Shell shellPage)
        {
            await shellPage.GoToAsync(route);
            return;
        }

        Page page = CreatePublicPage(route);
        if (window.Page is not NavigationPage navigation)
        {
            navigation = new NavigationPage(page);
            window.Page = navigation;
            return;
        }

        await navigation.PushAsync(page);
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
