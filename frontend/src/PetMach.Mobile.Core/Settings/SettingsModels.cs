namespace PetMach.Mobile.Core.Settings;

public sealed record SettingsSection(
    string Title,
    IReadOnlyList<SettingsSectionItem> Items);

public sealed record SettingsSectionItem(
    string Id,
    string Title,
    string Description,
    string? Route,
    bool IsEnabled,
    string? BadgeText = null)
{
    public bool HasBadge => !string.IsNullOrWhiteSpace(BadgeText);
    public bool ShowsChevron => IsEnabled && !string.IsNullOrWhiteSpace(Route);
    public string AccessibilityDescription => !IsEnabled
        ? $"{Title}, indisponível, {BadgeText ?? "em breve"}. {Description}"
        : $"{Title}. {Description}.";
}

public sealed record ConfirmationRequest(
    string Title,
    string Message,
    string AcceptText,
    string CancelText);

public interface IConfirmationService
{
    Task<bool> ConfirmAsync(
        ConfirmationRequest request,
        CancellationToken cancellationToken);
}

public sealed record AppInformation(
    string Name,
    string Version,
    string Build);

public interface IAppInformationProvider
{
    AppInformation GetCurrent();
}

public static class SettingsRoutes
{
    public const string Settings = "settings";
    public const string About = "about";
    public const string AboutFromSettings = "about?source=settings";

    private static readonly IReadOnlyList<string> routes =
        Array.AsReadOnly<string>([Settings, About]);

    public static IReadOnlyList<string> ShellRoutes => routes;
}
