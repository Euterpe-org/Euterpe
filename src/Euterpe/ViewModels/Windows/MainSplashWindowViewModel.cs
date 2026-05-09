using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.ViewModels.Windows;

public sealed class MainSplashWindowViewModel : ViewModelBase, IDialogContext
{
    public AsyncManualResetEvent Ready { get; } = new(false);

    public void Close()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<object?>? RequestClose;

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(true);

        var restored = await AuthService.RestoreSessionAsync().ConfigureAwait(true);
        if (!restored)
        {
            await AuthService.LoginAsync().ConfigureAwait(true);
        }

        await AuthService.Ready.WaitAsync().ConfigureAwait(true);

#if RELEASE
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await UpdateService.CheckForUpdatesAsync(cts.Token).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Update check failed during splash, continuing startup");
        }
#endif

        Ready.Set();
        Close();
    }

    #region Injections

    [UsedImplicitly]
    public required IAuthService AuthService { get; init; }

    [UsedImplicitly]
    public required ILogger<MainSplashWindowViewModel> Logger { get; init; }

#if RELEASE
    [UsedImplicitly]
    public required IUpdateService UpdateService { get; init; }
#endif

    #endregion Injections
}