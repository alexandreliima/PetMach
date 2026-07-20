using PetMach.Mobile.Core.Settings;

namespace PetMach.Mobile.Settings;

public sealed class MauiAppInformationProvider : IAppInformationProvider
{
    public AppInformation GetCurrent() =>
        new(
            AppInfo.Current.Name,
            AppInfo.Current.VersionString,
            AppInfo.Current.BuildString);
}
