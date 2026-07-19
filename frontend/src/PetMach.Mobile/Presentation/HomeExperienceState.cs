using CommunityToolkit.Mvvm.ComponentModel;

namespace PetMach.Mobile.Presentation;

// Temporary presentation-only data. Replace this state when a Home aggregation
// contract becomes available; no business rule or API behavior belongs here.
public sealed class HomeExperienceState : ObservableObject
{
    private bool isLoading = true;
    private bool hasError;
    private string errorMessage = string.Empty;
    private HomePetPreview? primaryPet;
    private IReadOnlyList<HomeMatchPreview> recentMatches = [];
    private IReadOnlyList<HomeAppointmentPreview> upcomingAppointments = [];
    private HomePartnerPreview? featuredPartner;

    public bool IsLoading
    {
        get => isLoading;
        private set
        {
            if (SetProperty(ref isLoading, value))
            {
                OnPropertyChanged(nameof(ShowContent));
            }
        }
    }

    public bool HasError
    {
        get => hasError;
        private set
        {
            if (SetProperty(ref hasError, value))
            {
                OnPropertyChanged(nameof(ShowContent));
            }
        }
    }

    public string ErrorMessage
    {
        get => errorMessage;
        private set => SetProperty(ref errorMessage, value);
    }

    public HomePetPreview? PrimaryPet
    {
        get => primaryPet;
        private set
        {
            if (SetProperty(ref primaryPet, value))
            {
                OnPropertyChanged(nameof(HasPrimaryPet));
                OnPropertyChanged(nameof(HasNoPrimaryPet));
            }
        }
    }

    public IReadOnlyList<HomeMatchPreview> RecentMatches
    {
        get => recentMatches;
        private set
        {
            if (SetProperty(ref recentMatches, value))
            {
                OnPropertyChanged(nameof(HasRecentMatches));
                OnPropertyChanged(nameof(HasNoRecentMatches));
            }
        }
    }

    public IReadOnlyList<HomeAppointmentPreview> UpcomingAppointments
    {
        get => upcomingAppointments;
        private set
        {
            if (SetProperty(ref upcomingAppointments, value))
            {
                OnPropertyChanged(nameof(HasUpcomingAppointments));
                OnPropertyChanged(nameof(HasNoUpcomingAppointments));
            }
        }
    }

    public HomePartnerPreview? FeaturedPartner
    {
        get => featuredPartner;
        private set
        {
            if (SetProperty(ref featuredPartner, value))
            {
                OnPropertyChanged(nameof(HasFeaturedPartner));
            }
        }
    }

    public bool ShowContent => !IsLoading && !HasError;
    public bool HasPrimaryPet => PrimaryPet is not null;
    public bool HasNoPrimaryPet => PrimaryPet is null;
    public bool HasRecentMatches => RecentMatches.Count > 0;
    public bool HasNoRecentMatches => RecentMatches.Count == 0;
    public bool HasUpcomingAppointments => UpcomingAppointments.Count > 0;
    public bool HasNoUpcomingAppointments => UpcomingAppointments.Count == 0;
    public bool HasFeaturedPartner => FeaturedPartner is not null;

    public void BeginLoading()
    {
        HasError = false;
        ErrorMessage = string.Empty;
        IsLoading = true;
    }

    public void LoadDemo()
    {
        PrimaryPet = new(
            "Thor",
            "Golden Retriever",
            "3 anos",
            "Amigável e cheio de energia",
            "petmatch_onboarding_dogs_android.jpg");
        RecentMatches =
        [
            new("Luna", "Border Collie", "Novo match hoje"),
            new("Mel", "Shih-tzu", "Conexão recente"),
        ];
        UpcomingAppointments =
        [
            new("Passeio no parque", "Amanhã · 09:00", "Parque da cidade"),
            new("Reserva confirmada", "Sábado · 10:30", "Espaço Au.Migo"),
        ];
        FeaturedPartner = new(
            "Espaço Au.Migo",
            "Daycare e experiências pet friendly",
            "Ambiente seguro · Atendimento especializado");
        HasError = false;
        ErrorMessage = string.Empty;
        IsLoading = false;
    }

    public void ShowRecoverableError()
    {
        IsLoading = false;
        ErrorMessage = "Não foi possível carregar sua experiência agora. Tente novamente.";
        HasError = true;
    }

    public void ShowEmpty()
    {
        PrimaryPet = null;
        RecentMatches = [];
        UpcomingAppointments = [];
        FeaturedPartner = null;
        HasError = false;
        ErrorMessage = string.Empty;
        IsLoading = false;
    }
}

public sealed record HomePetPreview(
    string Name,
    string Breed,
    string Age,
    string Description,
    string ImageSource);

public sealed record HomeMatchPreview(
    string Name,
    string Breed,
    string Status);

public sealed record HomeAppointmentPreview(
    string Title,
    string Schedule,
    string Place);

public sealed record HomePartnerPreview(
    string Name,
    string Description,
    string Highlights);
