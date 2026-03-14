namespace Euterpe.ViewModels.Panels.Modding;

public sealed partial class MelonLoaderPanelViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial InstallStatus MelonLoaderInstallStatus { get; set; }

    [ObservableProperty]
    public partial LocalizedString? DownloadText { get; set; }

    [ObservableProperty]
    public partial double DownloadProgress { get; set; }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync().ConfigureAwait(true);

        MelonLoaderInstallStatus = Config.MelonLoaderVersion is null ? InstallStatus.NotInstalled : InstallStatus.Installed;

        Logger.ZLogInformation($"{nameof(MelonLoaderPanelViewModel)} Initialized");
    }

    [RelayCommand]
    private async Task InstallMelonLoaderAsync()
    {
        try
        {
            MelonLoaderInstallStatus = InstallStatus.Downloading;

            var progress = new Progress<double>(value => DownloadProgress = value);
            await DependencyAcquireService.AcquireForMelonLoaderAsync(OnDownloadStarted, progress).ConfigureAwait(true);
            await LocalService.InstallMelonLoaderAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await MessageBoxService.ErrorAsync(MessageBox_Content_MelonLoader_Install_Failed, ex).ConfigureAwait(false);
            Logger.ZLogError(ex, $"Failed to install MelonLoader");
            MelonLoaderInstallStatus = InstallStatus.NotInstalled;
            return;
        }

        Config.MelonLoaderVersion = DependencyConstants.MelonLoader.Version;
        MelonLoaderInstallStatus = InstallStatus.Installed;
    }

    [RelayCommand]
    private async Task UninstallMelonLoaderAsync()
    {
        await LocalService.UninstallMelonLoaderAsync().ConfigureAwait(false);
        Config.MelonLoaderVersion = null;
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

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required IDependencyAcquireService DependencyAcquireService { get; init; }

    [UsedImplicitly]
    public required ILocalService LocalService { get; init; }

    [UsedImplicitly]
    public required ILogger<MelonLoaderPanelViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}