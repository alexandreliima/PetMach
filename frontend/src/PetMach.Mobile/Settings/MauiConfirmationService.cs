using PetMach.Mobile.Core.Settings;
using PetMach.Mobile.Navigation;

namespace PetMach.Mobile.Settings;

public sealed class MauiConfirmationService(
    RootNavigationService rootNavigation) : IConfirmationService
{
    public Task<bool> ConfirmAsync(
        ConfirmationRequest request,
        CancellationToken cancellationToken) =>
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            Page page = rootNavigation.GetWindow().Page
                ?? throw new InvalidOperationException("The application window has no active page.");
            return await page.DisplayAlertAsync(
                request.Title,
                request.Message,
                request.AcceptText,
                request.CancelText);
        });
}
