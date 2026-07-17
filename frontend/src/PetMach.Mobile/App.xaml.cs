using Microsoft.Extensions.DependencyInjection;

namespace PetMach.Mobile;

public partial class App : Application
{
    private readonly IServiceProvider services;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        this.services = services;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        MainPage mainPage = services.GetRequiredService<MainPage>();
        NavigationPage.SetHasNavigationBar(mainPage, false);
        NavigationPage navigation = new(mainPage)
        {
            BarBackgroundColor = Color.FromArgb("#FFF9F3"),
            BarTextColor = Color.FromArgb("#123C38"),
        };
        return new Window(navigation);
    }
}
