namespace MuseDashModTools.ViewModels.Panels.Modding;

public sealed partial class MelonLoaderPanelViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string? InstalledMelonLoaderVersion { get; set; }

    [ObservableProperty]
    public partial InstallStatus MelonLoaderInstallStatus { get; set; }

    [ObservableProperty]
    public partial LocalizedString? DownloadText { get; set; }

    [ObservableProperty]
    public partial double DownloadProgress { get; set; }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync().ConfigureAwait(true);

        InstalledMelonLoaderVersion = LocalService.ReadMelonLoaderVersion();
        MelonLoaderInstallStatus = InstalledMelonLoaderVersion is null ? InstallStatus.NotInstalled : InstallStatus.Installed;

        await CheckAndInstallDotNetRuntimeAsync().ConfigureAwait(false);

        Logger.ZLogInformation($"{nameof(MelonLoaderPanelViewModel)} Initialized");
    }

    [RelayCommand]
    private async Task InstallMelonLoaderAsync()
    {
        try
        {
            MelonLoaderInstallStatus = InstallStatus.Downloading;
            var downloadProgress = new Progress<double>(value => DownloadProgress = value);
            await DownloadManager
                .DownloadMelonLoaderAsync(OnDownloadStarted, downloadProgress)
                .ConfigureAwait(true);
            await LocalService.InstallMelonLoaderAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await MessageBoxService.ErrorAsync("Failed to install MelonLoader: {0}", ex).ConfigureAwait(false);
            Logger.ZLogError(ex, $"Failed to install MelonLoader");
            MelonLoaderInstallStatus = InstallStatus.NotInstalled;
            return;
        }

        InstalledMelonLoaderVersion = MelonLoaderVersion;
        MelonLoaderInstallStatus = InstallStatus.Installed;
    }

    [RelayCommand]
    private async Task UninstallMelonLoaderAsync()
    {
        await LocalService.UninstallMelonLoaderAsync().ConfigureAwait(false);
        InstalledMelonLoaderVersion = null;
        MelonLoaderInstallStatus = InstallStatus.NotInstalled;
        Logger.ZLogInformation($"MelonLoader has been successfully uninstalled");
    }

    private void OnDownloadStarted(object? sender, DownloadStartedEventArgs args)
    {
        var fileName = Path.GetFileName(args.FileName);
        var mbSize = args.TotalBytesToReceive / (1024d * 1024d);
        DownloadText = string.Format(XAML.MelonLoader_State_Downloading, fileName, $"{mbSize:F2}");
        Logger.ZLogInformation($"Downloading {fileName}: {args.TotalBytesToReceive}B");
    }

    private async Task CheckAndInstallDotNetRuntimeAsync()
    {
        var runtimeInstalled = await LocalService.CheckDotNetRuntimeInstalledAsync().ConfigureAwait(true);
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
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required IDownloadManager DownloadManager { get; init; }

    [UsedImplicitly]
    public required ILocalService LocalService { get; init; }

    [UsedImplicitly]
    public required ILogger<MelonLoaderPanelViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}