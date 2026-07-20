using FluentAssertions;
using PetMach.Mobile.Core.Identity;
using PetMach.Mobile.Core.Navigation;
using PetMach.Mobile.Core.Settings;

namespace PetMach.Mobile.Tests.Settings;

public sealed class SettingsViewModelTests
{
    [Fact]
    public async Task LoadShouldExposeAllSectionsAndItemStates()
    {
        SettingsViewModel viewModel = CreateViewModel();

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.Sections.Select(section => section.Title).Should().Equal(
            "Conta",
            "Preferências",
            "Privacidade e segurança",
            "Informações");
        SettingsSectionItem[] items = viewModel.Sections
            .SelectMany(section => section.Items)
            .ToArray();
        items.Should().HaveCount(9);
        items.Single(item => item.Id == "about").IsEnabled.Should().BeTrue();
        items.Where(item => item.Id != "about").Should().OnlyContain(item =>
            !item.IsEnabled &&
            item.BadgeText == "Em breve" &&
            item.Route == null &&
            item.AccessibilityDescription.Contains(
                "indisponível, Em breve",
                StringComparison.Ordinal));
    }

    [Fact]
    public async Task AboutShouldNavigateUsingTheSettingsContext()
    {
        RecordingNavigator navigator = new();
        SettingsViewModel viewModel = CreateViewModel(navigator: navigator);
        await viewModel.LoadCommand.ExecuteAsync(null);
        SettingsSectionItem about = viewModel.Sections
            .SelectMany(section => section.Items)
            .Single(item => item.Id == "about");

        await viewModel.OpenItemCommand.ExecuteAsync(about);

        navigator.Routes.Should().ContainSingle()
            .Which.Should().Be(SettingsRoutes.AboutFromSettings);
    }

    [Fact]
    public async Task DisabledItemShouldNotNavigate()
    {
        RecordingNavigator navigator = new();
        SettingsViewModel viewModel = CreateViewModel(navigator: navigator);
        await viewModel.LoadCommand.ExecuteAsync(null);
        SettingsSectionItem appearance = viewModel.Sections
            .SelectMany(section => section.Items)
            .Single(item => item.Id == "appearance");

        await viewModel.OpenItemCommand.ExecuteAsync(appearance);

        viewModel.OpenItemCommand.CanExecute(appearance).Should().BeFalse();
        navigator.Routes.Should().BeEmpty();
    }

    [Fact]
    public async Task CancelledConfirmationShouldNotLogout()
    {
        RecordingLogout logout = new();
        RecordingConfirmation confirmation = new(false);
        SettingsViewModel viewModel = CreateViewModel(
            confirmation: confirmation,
            logout: logout);

        await viewModel.LogoutCommand.ExecuteAsync(null);

        confirmation.Calls.Should().Be(1);
        logout.Calls.Should().Be(0);
        viewModel.IsLoggingOut.Should().BeFalse();
    }

    [Fact]
    public async Task ConfirmedLogoutShouldUseCoordinatorOnce()
    {
        RecordingLogout logout = new();
        RecordingConfirmation confirmation = new(true);
        SettingsViewModel viewModel = CreateViewModel(
            confirmation: confirmation,
            logout: logout);

        await viewModel.LogoutCommand.ExecuteAsync(null);

        confirmation.LastRequest.Should().Be(new ConfirmationRequest(
            "Sair da conta",
            "Deseja sair da sua conta? Será necessário entrar novamente para acessar o PetMatch.",
            "Sair",
            "Cancelar"));
        logout.Calls.Should().Be(1);
        logout.Tokens.Should().ContainSingle()
            .Which.IsCancellationRequested.Should().BeFalse();
    }

    [Fact]
    public async Task DelayedConfirmationShouldPassANonCancelledTokenToLogout()
    {
        RecordingLogout logout = new();
        BlockingConfirmation confirmation = new();
        SettingsViewModel viewModel = CreateViewModel(
            confirmation: confirmation,
            logout: logout);

        Task operation = viewModel.LogoutCommand.ExecuteAsync(null);
        await confirmation.Started.Task.WaitAsync(TestCancellation.Token);

        confirmation.Token.IsCancellationRequested.Should().BeFalse();
        confirmation.Release(true);
        await operation;

        logout.Tokens.Should().ContainSingle()
            .Which.IsCancellationRequested.Should().BeFalse();
    }

    [Fact]
    public async Task LogoutCommandShouldDisableReentryWhileConfirmationIsOpen()
    {
        RecordingLogout logout = new();
        BlockingConfirmation confirmation = new();
        SettingsViewModel viewModel = CreateViewModel(
            confirmation: confirmation,
            logout: logout);

        Task first = viewModel.LogoutCommand.ExecuteAsync(null);
        await confirmation.Started.Task.WaitAsync(TestCancellation.Token);

        viewModel.LogoutCommand.CanExecute(null).Should().BeFalse();
        confirmation.Calls.Should().Be(1);
        confirmation.Release(true);
        await first;

        confirmation.Calls.Should().Be(1);
        logout.Calls.Should().Be(1);
    }

    [Fact]
    public async Task LogoutShouldExposeProcessingState()
    {
        BlockingConfirmation confirmation = new();
        SettingsViewModel viewModel = CreateViewModel(confirmation: confirmation);

        Task logout = viewModel.LogoutCommand.ExecuteAsync(null);
        await confirmation.Started.Task.WaitAsync(TestCancellation.Token);

        viewModel.IsLoggingOut.Should().BeTrue();
        viewModel.LogoutButtonText.Should().Be("Saindo...");

        confirmation.Release(false);
        await logout;
        viewModel.IsLoggingOut.Should().BeFalse();
        viewModel.LogoutButtonText.Should().Be("Sair da conta");
    }

    [Fact]
    public async Task LogoutFailureShouldExposeFriendlyError()
    {
        RecordingLogout logout = new()
        {
            Exception = new InvalidOperationException("detalhe técnico"),
        };
        SettingsViewModel viewModel = CreateViewModel(
            confirmation: new RecordingConfirmation(true),
            logout: logout);

        await viewModel.LogoutCommand.ExecuteAsync(null);

        viewModel.HasError.Should().BeTrue();
        viewModel.ErrorMessage.Should().Be(
            "Não foi possível sair da conta. Tente novamente.");
        viewModel.ErrorMessage.Should().NotContain("detalhe técnico");
        viewModel.IsLoggingOut.Should().BeFalse();
    }

    [Fact]
    public async Task LogoutFailureShouldReleaseGuardAndAllowRetry()
    {
        RecordingLogout logout = new()
        {
            Exception = new InvalidOperationException("primeira tentativa"),
        };
        RecordingConfirmation confirmation = new(true);
        SettingsViewModel viewModel = CreateViewModel(
            confirmation: confirmation,
            logout: logout);

        await viewModel.LogoutCommand.ExecuteAsync(null);
        logout.Exception = null;
        await viewModel.LogoutCommand.ExecuteAsync(null);

        confirmation.Calls.Should().Be(2);
        logout.Calls.Should().Be(2);
        viewModel.IsLoggingOut.Should().BeFalse();
        viewModel.HasError.Should().BeFalse();
    }

    [Fact]
    public async Task CommandCancellationShouldRestoreStateWithoutCallingLogout()
    {
        RecordingLogout logout = new();
        BlockingConfirmation confirmation = new();
        SettingsViewModel viewModel = CreateViewModel(
            confirmation: confirmation,
            logout: logout);

        Task operation = viewModel.LogoutCommand.ExecuteAsync(null);
        await confirmation.Started.Task.WaitAsync(TestCancellation.Token);
        viewModel.LogoutCommand.Cancel();

        Func<Task> waitForCancellation = async () => await operation;
        await waitForCancellation.Should().ThrowAsync<OperationCanceledException>();
        viewModel.IsLoggingOut.Should().BeFalse();
        logout.Calls.Should().Be(0);
    }

    [Fact]
    public void ShellRouteCatalogShouldExposeTheConfigurationConsumedByAppShell()
    {
        SettingsRoutes.ShellRoutes.Should().Equal(
            SettingsRoutes.Settings,
            SettingsRoutes.About);
        SettingsRoutes.AboutFromSettings.Should().StartWith(
            $"{SettingsRoutes.About}?");
    }

    private static SettingsViewModel CreateViewModel(
        RecordingNavigator? navigator = null,
        IConfirmationService? confirmation = null,
        RecordingLogout? logout = null) =>
        new(
            navigator ?? new RecordingNavigator(),
            confirmation ?? new RecordingConfirmation(false),
            logout ?? new RecordingLogout());

    private sealed class RecordingNavigator : IMobileNavigator
    {
        public List<string> Routes { get; } = [];

        public Task GoToAsync(string route)
        {
            Routes.Add(route);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingConfirmation(bool result) : IConfirmationService
    {
        public int Calls { get; private set; }
        public ConfirmationRequest? LastRequest { get; private set; }

        public Task<bool> ConfirmAsync(
            ConfirmationRequest request,
            CancellationToken cancellationToken)
        {
            Calls++;
            LastRequest = request;
            return Task.FromResult(result);
        }
    }

    private sealed class BlockingConfirmation : IConfirmationService
    {
        private readonly TaskCompletionSource<bool> result =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int Calls { get; private set; }
        public CancellationToken Token { get; private set; }
        public TaskCompletionSource Started { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<bool> ConfirmAsync(
            ConfirmationRequest request,
            CancellationToken cancellationToken)
        {
            Calls++;
            Token = cancellationToken;
            Started.TrySetResult();
            return result.Task.WaitAsync(cancellationToken);
        }

        public void Release(bool confirmed) => result.TrySetResult(confirmed);
    }

    private sealed class RecordingLogout : ILogoutCoordinator
    {
        public int Calls { get; private set; }
        public Exception? Exception { get; set; }
        public List<CancellationToken> Tokens { get; } = [];

        public Task LogoutAsync(CancellationToken cancellationToken)
        {
            Calls++;
            Tokens.Add(cancellationToken);
            return Exception is null
                ? Task.CompletedTask
                : Task.FromException(Exception);
        }
    }
}
