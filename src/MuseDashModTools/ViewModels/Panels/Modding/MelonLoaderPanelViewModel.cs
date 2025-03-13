namespace MuseDashModTools.ViewModels.Panels.Modding;

public sealed partial class MelonLoaderPanelViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string? MelonLoaderVersion { get; set; }

    [ObservableProperty]
    public partial string? InstallStatus { get; set; }

    [ObservableProperty]
    public partial double DownloadProgress { get; set; }

    [ObservableProperty]
    public partial bool Downloading { get; set; }

    public override Task InitializeAsync()
    {
        base.InitializeAsync();

        MelonLoaderVersion = LocalService.ReadMelonLoaderVersion();

        Logger.ZLogInformation($"{nameof(MelonLoaderPanelViewModel)} Initialized");
        return Task.CompletedTask;
    }

    private void UpdateDownloadingStatus(DownloadStartedEventArgs args)
    {
        var fileName = Path.GetFileName(args.FileName);
        var mbSize = args.TotalBytesToReceive / (1024.0 * 1024.0);
        InstallStatus = $"Downloading {fileName}: {mbSize:F2}MB";
        Logger.ZLogInformation($"Downloading {fileName}: {args.TotalBytesToReceive}B");
    }

    [RelayCommand]
    private async Task InstallMelonLoaderAsync()
    {
        Downloading = true;
        InstallStatus = "Preparing...";

        try
        {
            var downloadProgress = new Progress<double>(value => DownloadProgress = value);
            await DownloadManager
                .DownloadMelonLoaderAsync(
                    (_, args) => UpdateDownloadingStatus(args),
                    downloadProgress)
                .ConfigureAwait(true);
            await LocalService.InstallMelonLoaderAsync().ConfigureAwait(false);
        }
        catch
        {
            Downloading = false;
            return;
        }

        MelonLoaderVersion = "0.6.1";
        Downloading = false;
    }

    [RelayCommand]
    private async Task UninstallMelonLoaderAsync()
    {
        // TODO

        MelonLoaderVersion = null;
        Logger.ZLogInformation($"MelonLoader has been successfully uninstalled");
    }

    [UsedImplicitly]
    partial void OnMelonLoaderVersionChanged(string? value)
    {
        InstallStatus = value.IsNullOrEmpty() ? "Not Installed" : "Installed";
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

    #endregion Injections
}