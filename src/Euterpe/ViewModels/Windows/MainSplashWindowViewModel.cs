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

#if RELEASE
        await UpdateService.CheckForUpdatesAsync().ConfigureAwait(true);
#endif
        await CheckAndInstallDotNetRuntimeAsync().ConfigureAwait(true);

        var restored = await AuthService.RestoreSessionAsync().ConfigureAwait(true);
        if (!restored)
        {
            await AuthService.LoginAsync().ConfigureAwait(true);
        }

        await AuthService.Ready.WaitAsync().ConfigureAwait(true);

        Ready.Set();
        Close();
    }

    private async Task CheckAndInstallDotNetRuntimeAsync()
    {
        var runtimeInstalled = await PlatformService.CheckDotNetRuntimeInstalledAsync().ConfigureAwait(true);
        if (runtimeInstalled)
        {
            return;
        }

        var result = await MessageBoxService.NoticeAsync(MessageBox_Content_DotNetRuntime_Install).ConfigureAwait(true);
        if (result is not MessageBoxResult.OK)
        {
            return;
        }

        var success = await PlatformService.InstallDotNetRuntimeAsync().ConfigureAwait(true);
        if (!success)
        {
            await MessageBoxService.ErrorAsync(MessageBox_Content_DotNetRuntime_Install_Failed).ConfigureAwait(false);
        }
    }

    #region Injections

    [UsedImplicitly]
    public required IAuthService AuthService { get; init; }

    [UsedImplicitly]
    public required ILogger<MainSplashWindowViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

#if RELEASE
    [UsedImplicitly]
    public required IUpdateService UpdateService { get; init; }
#endif

    #endregion Injections
}