using PetMach.Mobile.Core.Features;
using PetMach.Mobile.Presentation;

namespace PetMach.Mobile;

public partial class DogsPage : ContentPage
{
    private readonly DogsViewModel viewModel;
    private bool isSynchronizingSelection;

    public DogsPage(DogsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    public PetProfileExperienceState Presentation { get; } = new();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProfilesAsync();
    }

    private async Task LoadProfilesAsync()
    {
        Presentation.BeginLoading();
        await viewModel.LoadCommand.ExecuteAsync(null);

        if (viewModel.Dogs.Count == 0 &&
            viewModel.StatusMessage.Contains("poss", StringComparison.OrdinalIgnoreCase))
        {
            Presentation.ShowError(viewModel.StatusMessage);
            return;
        }

        DogModel? selectedDog = PetPicker.SelectedItem as DogModel ?? viewModel.Dogs.FirstOrDefault();
        isSynchronizingSelection = true;
        PetPicker.SelectedItem = selectedDog;
        isSynchronizingSelection = false;
        Presentation.LoadOwner(selectedDog);
        await AnimateContentAsync();
    }

    private async void RetryClicked(object sender, EventArgs e)
    {
        await LoadProfilesAsync();
    }

    private async void PetSelectionChanged(object sender, EventArgs e)
    {
        if (!isSynchronizingSelection &&
            Presentation.IsOwnerMode &&
            PetPicker.SelectedItem is DogModel dog)
        {
            Presentation.LoadOwner(dog);
            await AnimateContentAsync();
        }
    }

    private async void ShowVisitorClicked(object sender, EventArgs e)
    {
        Presentation.LoadVisitorDemo();
        await AnimateContentAsync();
    }

    private async void ShowOwnerClicked(object sender, EventArgs e)
    {
        DogModel? dog = PetPicker.SelectedItem as DogModel ?? viewModel.Dogs.FirstOrDefault();
        Presentation.LoadOwner(dog);
        await AnimateContentAsync();
    }

    private async Task AnimateContentAsync()
    {
        ProfileContent.Opacity = 0;
        ProfileContent.TranslationY = 10;
        await Task.WhenAll(
            ProfileContent.FadeToAsync(1, 240, Easing.CubicOut),
            ProfileContent.TranslateToAsync(0, 0, 280, Easing.CubicOut));
    }
}
