using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PetMach.Mobile.Core.Navigation;

namespace PetMach.Mobile.Core.Settings;

public sealed partial class AboutViewModel : ObservableObject
{
    private readonly IMobileNavigator navigator;

    [ObservableProperty]
    private bool showPublicActions = true;

    public AboutViewModel(
        IAppInformationProvider appInformation,
        IMobileNavigator navigator)
    {
        this.navigator = navigator;
        AppInformation current = appInformation.GetCurrent();
        AppName = ValueOrFallback(current.Name, "PetMatch");
        Version = ValueOrFallback(current.Version, "não informada");
        Build = ValueOrFallback(current.Build, "não informado");
        VersionDescription = $"Versão {Version} · Build {Build}";
    }

    public string AppName { get; }
    public string Version { get; }
    public string Build { get; }
    public string VersionDescription { get; }

    public void ApplySource(string? source) =>
        ShowPublicActions = !string.Equals(
            source,
            "settings",
            StringComparison.OrdinalIgnoreCase);

    [RelayCommand]
    private Task OpenLoginAsync() => navigator.GoToAsync("login");

    [RelayCommand]
    private Task OpenRegistrationAsync() => navigator.GoToAsync("register");

    private static string ValueOrFallback(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}
