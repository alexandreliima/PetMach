using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Core.Features;

public sealed partial class DogsViewModel(IPetMachApiClient api, IMobileNavigator navigator) : ObservableObject
{
    public ObservableCollection<DogModel> Dogs { get; } = [];
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = string.Empty;
    public bool HasDogs => Dogs.Count > 0;

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            Dogs.Clear();
            foreach (DogModel dog in await api.GetDogsAsync(CancellationToken.None)) Dogs.Add(dog);
            StatusMessage = Dogs.Count == 0 ? "Você ainda não cadastrou um cão." : string.Empty;
            OnPropertyChanged(nameof(HasDogs));
        }
        catch (AuthenticationRequiredException ex) { StatusMessage = ex.Message; }
        catch (HttpRequestException) { StatusMessage = "Não foi possível carregar seus cães."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private Task AddDogAsync() => navigator.GoToAsync("dog-form");
}
