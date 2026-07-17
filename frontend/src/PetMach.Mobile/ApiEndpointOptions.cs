namespace PetMach.Mobile;

public sealed record ApiEndpointOptions(Uri BaseAddress)
{
    private const string EnvironmentVariable = "PETMACH_API_BASE_URL";
    private const string AndroidEmulatorDebugUrl = "http://10.0.2.2:5049/";

    public static ApiEndpointOptions FromEnvironment()
    {
        string? configuredUrl = Environment.GetEnvironmentVariable(EnvironmentVariable);
        string url = string.IsNullOrWhiteSpace(configuredUrl)
            ? AndroidEmulatorDebugUrl
            : configuredUrl;

        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? baseAddress))
        {
            throw new InvalidOperationException(
                $"A variável {EnvironmentVariable} não contém uma URL absoluta válida.");
        }

        return new ApiEndpointOptions(baseAddress);
    }
}
