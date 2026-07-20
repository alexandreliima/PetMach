using PetMach.Mobile.Core.Settings;

namespace PetMach.Mobile;

public partial class AboutPage : ContentPage, IQueryAttributable
{
    private readonly AboutViewModel viewModel;

    public AboutPage(AboutViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query) =>
        viewModel.ApplySource(query.TryGetValue("source", out object? source)
            ? source?.ToString()
            : null);
}
