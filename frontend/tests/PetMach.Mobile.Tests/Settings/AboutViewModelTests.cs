using FluentAssertions;
using PetMach.Mobile.Core.Navigation;
using PetMach.Mobile.Core.Settings;

namespace PetMach.Mobile.Tests.Settings;

public sealed class AboutViewModelTests
{
    [Fact]
    public void AppInformationShouldExposeNameVersionBuildAndSafeFormatting()
    {
        AboutViewModel viewModel = new(
            new AppInformationProvider(new AppInformation(
                "PetMatch",
                "1.4.2",
                "37")),
            new RecordingNavigator());

        viewModel.AppName.Should().Be("PetMatch");
        viewModel.Version.Should().Be("1.4.2");
        viewModel.Build.Should().Be("37");
        viewModel.VersionDescription.Should().Be("Versão 1.4.2 · Build 37");
    }

    [Fact]
    public void MissingMetadataShouldUseSafeFallbacks()
    {
        AboutViewModel viewModel = new(
            new AppInformationProvider(new AppInformation(" ", "", "  ")),
            new RecordingNavigator());

        viewModel.AppName.Should().Be("PetMatch");
        viewModel.VersionDescription.Should().Be(
            "Versão não informada · Build não informado");
    }

    [Fact]
    public void SettingsContextShouldHidePublicAuthenticationActions()
    {
        AboutViewModel viewModel = CreateViewModel();

        viewModel.ApplySource("settings");

        viewModel.ShowPublicActions.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("public")]
    public void PublicContextShouldRestoreAuthenticationActions(string? source)
    {
        AboutViewModel viewModel = CreateViewModel();
        viewModel.ApplySource("settings");

        viewModel.ApplySource(source);

        viewModel.ShowPublicActions.Should().BeTrue();
    }

    [Fact]
    public async Task PublicActionsShouldKeepExistingNavigation()
    {
        RecordingNavigator navigator = new();
        AboutViewModel viewModel = CreateViewModel(navigator);

        await viewModel.OpenRegistrationCommand.ExecuteAsync(null);
        await viewModel.OpenLoginCommand.ExecuteAsync(null);

        navigator.Routes.Should().Equal("register", "login");
    }

    private static AboutViewModel CreateViewModel(
        RecordingNavigator? navigator = null) =>
        new(
            new AppInformationProvider(new AppInformation(
                "PetMatch",
                "1.0",
                "1")),
            navigator ?? new RecordingNavigator());

    private sealed class AppInformationProvider(AppInformation information)
        : IAppInformationProvider
    {
        public AppInformation GetCurrent() => information;
    }

    private sealed class RecordingNavigator : IMobileNavigator
    {
        public List<string> Routes { get; } = [];

        public Task GoToAsync(string route)
        {
            Routes.Add(route);
            return Task.CompletedTask;
        }
    }
}
