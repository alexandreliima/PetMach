namespace PetMach.Mobile;

public sealed class AppShell : Shell
{
    private static int routesRegistered;

    public AppShell(
        DiscoveryPage discoveryPage,
        MatchesPage matchesPage,
        ConversationsPage conversationsPage,
        HomePage homePage)
    {
        FlyoutBehavior = FlyoutBehavior.Disabled;
        FlyoutIsPresented = false;
        FlyoutBackdrop = Brush.Transparent;

        TabBar app = new() { Route = "app" };
        app.Items.Add(new ShellContent
        {
            Title = "Descobrir",
            Route = "network",
            Content = discoveryPage,
            Icon = "tab_discover.svg",
        });
        app.Items.Add(new ShellContent
        {
            Title = "Encontros",
            Route = "matches-tab",
            Content = matchesPage,
            Icon = "tab_meetings.svg",
        });
        app.Items.Add(new ShellContent
        {
            Title = "Conversas",
            Route = "conversations-tab",
            Content = conversationsPage,
            Icon = "tab_chat.svg",
        });
        app.Items.Add(new ShellContent
        {
            Title = "Perfil",
            Route = "menu",
            Content = homePage,
            Icon = "tab_profile.svg",
        });
        Items.Add(app);

        if (Interlocked.Exchange(ref routesRegistered, 1) == 0)
        {
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
            Routing.RegisterRoute("adoption", typeof(AdoptionPage));
        }
    }
}
