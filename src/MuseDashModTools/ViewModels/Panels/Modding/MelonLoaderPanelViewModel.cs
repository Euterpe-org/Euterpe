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

    public override Task InitializeAsync()
    {
        base.InitializeAsync();

        InstalledMelonLoaderVersion = LocalService.ReadMelonLoaderVersion();
        MelonLoaderInstallStatus = InstalledMelonLoaderVersion is null ? InstallStatus.NotInstalled : InstallStatus.Installed;

        Logger.ZLogInformation($"{nameof(MelonLoaderPanelViewModel)} Initialized");
        return Task.CompletedTask;
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
        var mbSize = args.TotalBytesToReceive / (1024.0 * 1024.0);
        DownloadText = string.Format(XAML.MelonLoader_State_Downloading, fileName, $"{mbSize:F2}");
        Logger.ZLogInformation($"Downloading {fileName}: {args.TotalBytesToReceive}B");
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