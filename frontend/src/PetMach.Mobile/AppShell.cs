namespace PetMach.Mobile;

public sealed class AppShell : Shell
{
    public AppShell(MainPage mainPage, HomePage homePage)
    {
        FlyoutBehavior = FlyoutBehavior.Disabled;
        Shell.SetTabBarIsVisible(this, false);
        Items.Add(new ShellContent
        {
            Title = "Boas-vindas",
            Route = "welcome",
            Content = mainPage,
        });
        Items.Add(new ShellContent
        {
            Title = "Início",
            Route = "home",
            Content = homePage,
        });
        Routing.RegisterRoute("login", typeof(LoginPage));
        Routing.RegisterRoute("register", typeof(RegisterPage));
        Routing.RegisterRoute("about", typeof(AboutPage));
        Routing.RegisterRoute("tutor-profile", typeof(TutorProfilePage));
        Routing.RegisterRoute("dogs", typeof(DogsPage));
        Routing.RegisterRoute("dog-form", typeof(DogFormPage));
        Routing.RegisterRoute("health", typeof(HealthPage));
        Routing.RegisterRoute("discovery", typeof(DiscoveryPage));
        Routing.RegisterRoute("matches", typeof(MatchesPage));
        Routing.RegisterRoute("notifications", typeof(NotificationsPage));
        Routing.RegisterRoute("conversations", typeof(ConversationsPage));
        Routing.RegisterRoute("chat", typeof(ChatPage));
        Routing.RegisterRoute("meetings", typeof(MeetingsPage));
        Routing.RegisterRoute("partner-spaces", typeof(PartnerSpacesPage));
        Routing.RegisterRoute("reservations", typeof(ReservationsPage));
        Routing.RegisterRoute("partner-operations", typeof(PartnerOperationsPage));
    }
}
