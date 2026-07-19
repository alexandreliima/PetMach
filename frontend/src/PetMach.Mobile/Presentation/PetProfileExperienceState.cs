using CommunityToolkit.Mvvm.ComponentModel;
using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile.Presentation;

// Presentation-only composition for PTM-007. Public preview data is intentionally
// isolated here until an explicit pet profile contract and route are available.
public sealed class PetProfileExperienceState : ObservableObject
{
    private bool isLoading = true;
    private bool hasError;
    private string errorMessage = string.Empty;
    private bool isOwnerMode = true;
    private bool hasProfile;
    private string name = string.Empty;
    private string species = "Cão";
    private string breed = string.Empty;
    private string age = string.Empty;
    private string sex = string.Empty;
    private string approximateLocation = string.Empty;
    private string heroImageSource = "petmach_welcome_dogs.png";
    private bool hasMainPhoto;
    private string description = string.Empty;
    private string healthSummary = string.Empty;
    private string compatibilitySummary = string.Empty;
    private IReadOnlyList<string> gallery = [];
    private IReadOnlyList<string> characteristics = [];
    private IReadOnlyList<string> personality = [];
    private IReadOnlyList<PetProfileStatistic> statistics = [];
    private IReadOnlyList<PetProfileTimelineItem> timeline = [];

    public bool IsLoading
    {
        get => isLoading;
        private set
        {
            if (SetProperty(ref isLoading, value))
            {
                OnPropertyChanged(nameof(ShowContent));
                OnPropertyChanged(nameof(ShowNoProfile));
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
                OnPropertyChanged(nameof(ShowNoProfile));
            }
        }
    }

    public string ErrorMessage
    {
        get => errorMessage;
        private set => SetProperty(ref errorMessage, value);
    }

    public bool IsOwnerMode
    {
        get => isOwnerMode;
        private set
        {
            if (SetProperty(ref isOwnerMode, value))
            {
                OnPropertyChanged(nameof(IsVisitorMode));
                OnPropertyChanged(nameof(ModeLabel));
                OnPropertyChanged(nameof(ShowIncompleteOwner));
            }
        }
    }

    public bool IsVisitorMode => !IsOwnerMode;
    public string ModeLabel => IsOwnerMode ? "PERFIL PRÓPRIO" : "PERFIL PÚBLICO";

    public bool HasProfile
    {
        get => hasProfile;
        private set
        {
            if (SetProperty(ref hasProfile, value))
            {
                OnPropertyChanged(nameof(HasNoProfile));
                OnPropertyChanged(nameof(ShowContent));
                OnPropertyChanged(nameof(ShowNoProfile));
            }
        }
    }

    public bool HasNoProfile => !HasProfile;
    public bool ShowContent => !IsLoading && !HasError && HasProfile;
    public bool ShowNoProfile => !IsLoading && !HasError && !HasProfile;

    public string Name { get => name; private set => SetProperty(ref name, value); }
    public string Species { get => species; private set => SetProperty(ref species, value); }
    public string Breed { get => breed; private set => SetProperty(ref breed, value); }
    public string Age { get => age; private set => SetProperty(ref age, value); }
    public string Sex { get => sex; private set => SetProperty(ref sex, value); }
    public string ApproximateLocation { get => approximateLocation; private set => SetProperty(ref approximateLocation, value); }
    public string HeroImageSource { get => heroImageSource; private set => SetProperty(ref heroImageSource, value); }

    public bool HasMainPhoto
    {
        get => hasMainPhoto;
        private set
        {
            if (SetProperty(ref hasMainPhoto, value))
            {
                OnPropertyChanged(nameof(HasNoMainPhoto));
                OnPropertyChanged(nameof(IsProfileIncomplete));
                OnPropertyChanged(nameof(ShowIncompleteOwner));
            }
        }
    }

    public bool HasNoMainPhoto => !HasMainPhoto;

    public string Description
    {
        get => description;
        private set
        {
            if (SetProperty(ref description, value))
            {
                OnPropertyChanged(nameof(HasDescription));
                OnPropertyChanged(nameof(HasNoDescription));
                OnPropertyChanged(nameof(IsProfileIncomplete));
                OnPropertyChanged(nameof(ShowIncompleteOwner));
            }
        }
    }

    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
    public bool HasNoDescription => !HasDescription;
    public bool IsProfileIncomplete => HasNoMainPhoto || HasNoDescription;
    public bool ShowIncompleteOwner => IsOwnerMode && IsProfileIncomplete;

    public string HealthSummary { get => healthSummary; private set => SetProperty(ref healthSummary, value); }
    public string CompatibilitySummary { get => compatibilitySummary; private set => SetProperty(ref compatibilitySummary, value); }

    public IReadOnlyList<string> Gallery
    {
        get => gallery;
        private set
        {
            if (SetProperty(ref gallery, value))
            {
                OnPropertyChanged(nameof(HasGallery));
                OnPropertyChanged(nameof(HasNoGallery));
            }
        }
    }

    public bool HasGallery => Gallery.Count > 0;
    public bool HasNoGallery => Gallery.Count == 0;

    public IReadOnlyList<string> Characteristics
    {
        get => characteristics;
        private set => SetProperty(ref characteristics, value);
    }

    public IReadOnlyList<string> Personality
    {
        get => personality;
        private set => SetProperty(ref personality, value);
    }

    public IReadOnlyList<PetProfileStatistic> Statistics
    {
        get => statistics;
        private set => SetProperty(ref statistics, value);
    }

    public IReadOnlyList<PetProfileTimelineItem> Timeline
    {
        get => timeline;
        private set
        {
            if (SetProperty(ref timeline, value))
            {
                OnPropertyChanged(nameof(HasTimeline));
                OnPropertyChanged(nameof(HasNoTimeline));
            }
        }
    }

    public bool HasTimeline => Timeline.Count > 0;
    public bool HasNoTimeline => Timeline.Count == 0;

    public void BeginLoading()
    {
        HasError = false;
        ErrorMessage = string.Empty;
        IsLoading = true;
    }

    public void LoadOwner(DogModel? dog)
    {
        IsOwnerMode = true;
        HasError = false;
        ErrorMessage = string.Empty;
        IsLoading = false;

        if (dog is null)
        {
            HasProfile = false;
            return;
        }

        HasProfile = true;
        Name = dog.Name;
        Species = "Cão";
        Breed = dog.Breed;
        Age = FormatAge(dog.BirthDate, dog.ApproximateAge);
        Sex = dog.Sex == DogSexModel.Female ? "Fêmea" : "Macho";
        ApproximateLocation = "Sua localização permanece protegida";
        HeroImageSource = "petmach_welcome_dogs.png";
        HasMainPhoto = false;
        Description = dog.Biography ?? string.Empty;
        Gallery = [];
        Characteristics =
        [
            FormatSize(dog.Size),
            dog.WeightKg is null ? "Peso não informado" : $"{dog.WeightKg:0.#} kg",
            dog.Neutered ? "Castrado" : "Não castrado",
            FormatGoal(dog.Goal),
        ];
        Personality =
        [
            dog.Temperament,
            $"Energia {FormatEnergy(dog.EnergyLevel).ToLowerInvariant()}",
            $"Sociabilidade com pets {dog.SociabilityWithDogs}/5",
            $"Sociabilidade com pessoas {dog.SociabilityWithPeople}/5",
        ];
        HealthSummary = "Os dados detalhados de saúde permanecem privados e disponíveis somente na área de cuidados.";
        CompatibilitySummary = $"Objetivo principal: {FormatGoal(dog.Goal)}. Perfil com energia {FormatEnergy(dog.EnergyLevel).ToLowerInvariant()}.";
        Statistics =
        [
            new("—", "Matches"),
            new("—", "Encontros"),
            new("—", "Avaliações"),
        ];
        Timeline = [];
    }

    public void LoadVisitorDemo()
    {
        IsOwnerMode = false;
        HasProfile = true;
        HasError = false;
        ErrorMessage = string.Empty;
        IsLoading = false;
        Name = "Thor";
        Species = "Cão";
        Breed = "Golden Retriever";
        Age = "3 anos";
        Sex = "Macho";
        ApproximateLocation = "São Paulo, SP · região aproximada";
        HeroImageSource = "petmatch_onboarding_dogs_android.jpg";
        HasMainPhoto = true;
        Description = "Thor adora passeios, brincadeiras ao ar livre e conhecer novos amigos com calma e respeito.";
        Gallery =
        [
            "petmatch_onboarding_dogs_android.jpg",
            "petmach_welcome_dogs.png",
            "petmatch_splash_scene_android.jpg",
        ];
        Characteristics = ["Porte grande", "Castrado", "Amizade", "Vacinação em dia"];
        Personality = ["Amigável", "Brincalhão", "Sociável", "Energia moderada"];
        HealthSummary = "Vacinação em dia. Informações médicas detalhadas não são públicas.";
        CompatibilitySummary = "Gosta de passeios, socialização gradual e pets com energia moderada.";
        Statistics =
        [
            new("8", "Matches"),
            new("5", "Encontros"),
            new("12", "Conexões"),
        ];
        Timeline =
        [
            new("Novo match", "Thor criou uma nova conexão.", "Hoje"),
            new("Passeio concluído", "Encontro em espaço parceiro.", "Há 5 dias"),
            new("Perfil atualizado", "Novas características públicas.", "Há 2 semanas"),
        ];
    }

    public void ShowError(string message)
    {
        IsLoading = false;
        ErrorMessage = message;
        HasError = true;
    }

    private static string FormatAge(DateOnly? birthDate, bool approximate)
    {
        if (birthDate is null)
        {
            return "Idade não informada";
        }

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        int years = today.Year - birthDate.Value.Year;
        if (birthDate.Value > today.AddYears(-years))
        {
            years--;
        }

        string suffix = approximate ? " aproximadamente" : string.Empty;
        return years == 1 ? $"1 ano{suffix}" : $"{Math.Max(0, years)} anos{suffix}";
    }

    private static string FormatSize(DogSizeModel size) => size switch
    {
        DogSizeModel.Small => "Porte pequeno",
        DogSizeModel.Medium => "Porte médio",
        DogSizeModel.Large => "Porte grande",
        DogSizeModel.Giant => "Porte gigante",
        _ => "Porte não informado",
    };

    private static string FormatEnergy(EnergyLevelModel energy) => energy switch
    {
        EnergyLevelModel.Low => "Baixa",
        EnergyLevelModel.Moderate => "Moderada",
        EnergyLevelModel.High => "Alta",
        _ => "Não informada",
    };

    private static string FormatGoal(DogGoalModel goal) => goal switch
    {
        DogGoalModel.Friendship => "Amizade",
        DogGoalModel.Socialization => "Socialização",
        DogGoalModel.Walks => "Passeios",
        DogGoalModel.Events => "Eventos",
        DogGoalModel.Adoption => "Adoção",
        _ => "Não informado",
    };
}

public sealed record PetProfileStatistic(string Value, string Label);
public sealed record PetProfileTimelineItem(string Title, string Description, string When);
