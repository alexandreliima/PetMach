using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile;

public partial class DiscoveryPage : ContentPage, IDisposable
{
    private readonly DiscoveryViewModel viewModel;
    private readonly DiscoveryPageLifecycle lifecycle;

    public DiscoveryPage(DiscoveryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
        lifecycle = new DiscoveryPageLifecycle(viewModel);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await lifecycle.AppearAsync();
        }
        catch (OperationCanceledException) when (!lifecycle.IsActive)
        {
        }
    }

    protected override void OnDisappearing()
    {
        lifecycle.Disappear();
        base.OnDisappearing();
    }

    private async void SourceDogChanged(object sender, EventArgs e)
    {
        CancellationToken? lifetime = lifecycle.CurrentToken;
        if (sender is not Picker { SelectedItem: DogModel dog } ||
            viewModel.SelectedDog?.Id == dog.Id ||
            !lifetime.HasValue)
        {
            return;
        }

        try
        {
            await viewModel.SelectDogAsync(dog, lifetime.Value);
        }
        catch (OperationCanceledException) when (!lifecycle.IsActive)
        {
        }
    }

    public void Dispose()
    {
        lifecycle.Dispose();
        GC.SuppressFinalize(this);
    }
}
