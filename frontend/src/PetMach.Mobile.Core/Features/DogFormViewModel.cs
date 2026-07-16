using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Core.Features;

public sealed partial class DogFormViewModel(IPetMachApiClient api, IDeviceFilePicker files, IMobileNavigator navigator) : ObservableObject
{
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private DateTime birthDate = DateTime.Today.AddYears(-1);
    [ObservableProperty] private bool approximateAge;
    [ObservableProperty] private int sexIndex;
    [ObservableProperty] private string breed = string.Empty;
    [ObservableProperty] private int sizeIndex = 1;
    [ObservableProperty] private string weightKg = string.Empty;
    [ObservableProperty] private bool neutered;
    [ObservableProperty] private string temperament = string.Empty;
    [ObservableProperty] private int energyIndex = 1;
    [ObservableProperty] private int goalIndex;
    [ObservableProperty] private string biography = string.Empty;
    [ObservableProperty] private string selectedPhotoName = "Nenhuma foto selecionada";
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = string.Empty;
    private PickedFile? photo;

    [RelayCommand]
    private async Task PickPhotoAsync()
    {
        photo = await files.PickPhotoAsync(CancellationToken.None);
        SelectedPhotoName = photo?.FileName ?? "Nenhuma foto selecionada";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy) return;
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Breed) || string.IsNullOrWhiteSpace(Temperament))
        {
            StatusMessage = "Preencha nome, raça e temperamento.";
            return;
        }
        if (!string.IsNullOrWhiteSpace(WeightKg) && !decimal.TryParse(WeightKg, out _))
        {
            StatusMessage = "Informe um peso válido.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Salvando cão...";
            decimal? weight = decimal.TryParse(WeightKg, out decimal parsed) ? parsed : null;
            DogInput input = new(Name.Trim(), DateOnly.FromDateTime(BirthDate), ApproximateAge, (DogSexModel)Math.Max(0, SexIndex), Breed.Trim(), (DogSizeModel)Math.Max(0, SizeIndex), weight, Neutered, Temperament.Trim(), (EnergyLevelModel)Math.Max(0, EnergyIndex), 3, 3, 3, null, null, Clean(Biography), (DogGoalModel)Math.Max(0, GoalIndex));
            DogModel dog = await api.CreateDogAsync(input, CancellationToken.None);
            if (photo is not null) _ = await api.UploadDogPhotoAsync(dog.Id, photo, CancellationToken.None);
            StatusMessage = "Cão cadastrado com sucesso.";
            await navigator.GoToAsync("..");
        }
        catch (AuthenticationRequiredException ex) { StatusMessage = ex.Message; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível cadastrar o cão. Confira os dados e a API."; }
        finally { IsBusy = false; }
    }

    private static string? Clean(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
