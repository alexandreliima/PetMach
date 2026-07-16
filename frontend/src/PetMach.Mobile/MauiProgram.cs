using PetMach.Mobile.Core.Features;
using PetMach.Mobile.Core.Home;
using PetMach.Mobile.Core.Identity;
using PetMach.Mobile.Core.Navigation;
using PetMach.Mobile.Features;
using PetMach.Mobile.Identity;
using PetMach.Mobile.Navigation;

namespace PetMach.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(_ => { });

        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<HomePage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<AboutPage>();
        builder.Services.AddTransient<TutorProfilePage>();
        builder.Services.AddTransient<DogsPage>();
        builder.Services.AddTransient<DogFormPage>();
        builder.Services.AddTransient<HealthPage>();
        builder.Services.AddTransient<DiscoveryPage>();
        builder.Services.AddTransient<MatchesPage>();
        builder.Services.AddTransient<NotificationsPage>();
        builder.Services.AddTransient<ConversationsPage>();
        builder.Services.AddTransient<ChatPage>();
        builder.Services.AddTransient<MeetingsPage>();
        builder.Services.AddTransient<PartnerSpacesPage>();
        builder.Services.AddTransient<ReservationsPage>();
        builder.Services.AddTransient<PartnerOperationsPage>();
        builder.Services.AddSingleton<IMobileNavigator, ShellNavigator>();
        builder.Services.AddTransient<WelcomeViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<TutorProfileViewModel>();
        builder.Services.AddTransient<DogsViewModel>();
        builder.Services.AddTransient<DogFormViewModel>();
        builder.Services.AddTransient<HealthViewModel>();
        builder.Services.AddTransient<DiscoveryViewModel>();
        builder.Services.AddTransient<MatchesViewModel>();
        builder.Services.AddTransient<NotificationsViewModel>();
        builder.Services.AddTransient<ConversationsViewModel>();
        builder.Services.AddTransient<ChatViewModel>();
        builder.Services.AddTransient<MeetingsViewModel>();
        builder.Services.AddTransient<PartnerSpacesViewModel>();
        builder.Services.AddTransient<ReservationsViewModel>();
        builder.Services.AddTransient<PartnerOperationsViewModel>();
        builder.Services.AddTransient<IChatRealtimeClient, SignalRChatRealtimeClient>();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri("http://10.0.2.2:5049/"), Timeout = TimeSpan.FromSeconds(30) });
        builder.Services.AddSingleton<IAuthApiClient, AuthApiClient>();
        builder.Services.AddSingleton<ITokenStore, SecureTokenStore>();
        builder.Services.AddSingleton<AuthenticationSession>();
        builder.Services.AddSingleton<IPetMachApiClient, PetMachApiClient>();
        builder.Services.AddSingleton<IDeviceFilePicker, DeviceFilePicker>();

        return builder.Build();
    }
}
