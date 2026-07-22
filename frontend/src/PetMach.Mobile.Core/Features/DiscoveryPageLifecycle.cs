namespace PetMach.Mobile.Core.Features;

public sealed class DiscoveryPageLifecycle(DiscoveryViewModel viewModel) : IDisposable
{
    private readonly object sync = new();
    private CancellationTokenSource? presenceCancellation;
    private Task? activationTask;

    public bool IsActive
    {
        get
        {
            lock (sync)
            {
                return presenceCancellation is not null;
            }
        }
    }

    public CancellationToken? CurrentToken
    {
        get
        {
            lock (sync)
            {
                return presenceCancellation?.Token;
            }
        }
    }

    public Task AppearAsync()
    {
        lock (sync)
        {
            if (presenceCancellation is not null)
            {
                return activationTask ?? Task.CompletedTask;
            }

            presenceCancellation = new CancellationTokenSource();
            activationTask = viewModel.ActivateAsync(presenceCancellation.Token);
            return activationTask;
        }
    }

    public void Disappear()
    {
        CancellationTokenSource? previous;
        lock (sync)
        {
            previous = presenceCancellation;
            presenceCancellation = null;
            activationTask = null;
        }

        if (previous is null)
        {
            return;
        }

        viewModel.Deactivate();
        previous.Cancel();
        previous.Dispose();
    }

    public void Dispose()
    {
        Disappear();
        GC.SuppressFinalize(this);
    }
}
