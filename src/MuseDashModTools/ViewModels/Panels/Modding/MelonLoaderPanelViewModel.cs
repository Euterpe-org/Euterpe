namespace MuseDashModTools.ViewModels.Panels.Modding;

public sealed partial class MelonLoaderPanelViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string? MelonLoaderVersion { get; set; }

    [ObservableProperty]
    public partial double DownloadProgress { get; set; }

    public override Task InitializeAsync()
    {
        base.InitializeAsync();

        MelonLoaderVersion = LocalService.ReadMelonLoaderVersion();

        Logger.ZLogInformation($"{nameof(MelonLoaderPanelViewModel)} Initialized");
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task InstallMelonLoaderAsync()
    {
        var downloadProgress = new Progress<double>(value => DownloadProgress = value);
        await DownloadManager
            .DownloadMelonLoaderAsync((_, args) => Logger.ZLogInformation($"Downloading File Size: {args.TotalBytesToReceive}"), downloadProgress)
            .ConfigureAwait(true);
        await LocalService.InstallMelonLoaderAsync().ConfigureAwait(false);
        MelonLoaderVersion = "0.6.1";
        Logger.ZLogInformation($"MelonLoader has been successfully installed");
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
        if (value.IsNullOrEmpty())
        {
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

    #endregion Injections
}