namespace PetMach.Mobile.Tests;

internal static class TestCancellation
{
    private static readonly CancellationTokenSource Timeout = new(TimeSpan.FromMinutes(2));

    public static CancellationToken Token => Timeout.Token;
}
