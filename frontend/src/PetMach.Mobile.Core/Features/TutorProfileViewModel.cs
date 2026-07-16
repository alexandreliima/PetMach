using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Core.Features;

public sealed partial class TutorProfileViewModel(IPetMachApiClient api, IMobileNavigator navigator) : ObservableObject
{
    [ObservableProperty] private string firstName = string.Empty;
    [ObservableProperty] private string lastName = string.Empty;
    [ObservableProperty] private string phone = string.Empty;
    [ObservableProperty] private string city = string.Empty;
    [ObservableProperty] private string state = string.Empty;
    [ObservableProperty] private string biography = string.Empty;
    [ObservableProperty] private bool showCity;
    [ObservableProperty] private bool allowDiscovery = true;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = string.Empty;

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            TutorProfileModel? profile = await api.GetTutorProfileAsync(CancellationToken.None);
            if (profile is null) return;
            FirstName = profile.FirstName;
            LastName = profile.LastName;
            Phone = profile.Phone ?? string.Empty;
            City = profile.City;
            State = profile.State;
            Biography = profile.Biography ?? string.Empty;
            ShowCity = profile.ShowCity;
            AllowDiscovery = profile.AllowDiscovery;
        }
        catch (AuthenticationRequiredException ex) { StatusMessage = ex.Message; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível carregar o perfil. Confira se a API está ligada."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy) return;
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(City) || string.IsNullOrWhiteSpace(State))
        {
            StatusMessage = "Preencha nome, sobrenome, cidade e estado.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Salvando...";
            _ = await api.SaveTutorProfileAsync(new TutorProfileInput(FirstName.Trim(), LastName.Trim(), Clean(Phone), City.Trim(), State.Trim(), Clean(Biography), ShowCity, AllowDiscovery), CancellationToken.None);
            StatusMessage = string.Empty;
            await navigator.GoToAsync("//home");
        }
        catch (AuthenticationRequiredException ex) { StatusMessage = ex.Message; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível salvar. Confira os dados e a conexão com a API."; }
        finally { IsBusy = false; }
    }

    private static string? Clean(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
