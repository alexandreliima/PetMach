using Microsoft.Extensions.DependencyInjection;

namespace PetMach.Mobile;

public partial class App : Application
{
    private readonly AppShell appShell;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        appShell = services.GetRequiredService<AppShell>();
    }

    protected override Window CreateWindow(IActivationState? activationState) => new(appShell);
}
