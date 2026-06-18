using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.Shell;

[Register]
[AppSingleton]
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
        if (!restored && !await EnsureLoggedInAsync().ConfigureAwait(true))
        {
            return;
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

    /// <summary>
    ///     Drive the login flow until it succeeds, prompting to retry or quit on each failure.
    /// </summary>
    /// <returns>True once logged in; false if the user chose to quit (shutdown initiated).</returns>
    private async Task<bool> EnsureLoggedInAsync()
    {
        while (true)
        {
            await AuthService.LoginAsync().ConfigureAwait(true);
            if (AuthService.Ready.IsSet)
            {
                return true;
            }

            var result = await MessageBoxService.WarningConfirmAsync(MessageBox_Content_Login_Failed).ConfigureAwait(true);
            if (result is MessageBoxResult.Yes)
            {
                continue;
            }

            Ready.Set();
            GetCurrentDesktop().Shutdown();
            return false;
        }
    }

    #region Injections

    public required IAuthService AuthService { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }
    public required ILogger<MainSplashWindowViewModel> Logger { get; init; }
#if RELEASE
    public required IUpdateService UpdateService { get; init; }
#endif

    #endregion Injections
}
