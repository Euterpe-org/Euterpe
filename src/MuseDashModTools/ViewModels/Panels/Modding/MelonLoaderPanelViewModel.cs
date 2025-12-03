namespace MuseDashModTools.ViewModels.Panels.Modding;

public sealed partial class MelonLoaderPanelViewModel : ViewModelBase
{
    private const int MaxRetries = 3;

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
        await CheckAndInstallDotNetRuntimeAsync().ConfigureAwait(false);

        Logger.ZLogInformation($"{nameof(MelonLoaderPanelViewModel)} Initialized");
    }

    [RelayCommand]
    private async Task InstallMelonLoaderAsync()
    {
        try
        {
            MelonLoaderInstallStatus = InstallStatus.Downloading;

            await EnsureValidFileAsync(
                Config.MelonLoaderZipPath,
                MelonLoaderZipHash,
                DownloadManager.DownloadMelonLoaderAsync,
                "MelonLoader").ConfigureAwait(true);

            await EnsureValidFileAsync(
                Config.UnityDependencyZipPath,
                UnityDependencyZipHash,
                DownloadManager.DownloadUnityDependencyAsync,
                "UnityDependency").ConfigureAwait(true);

            await EnsureValidFileAsync(
                Config.Cpp2ILZipPath,
                Cpp2ILZipHash,
                DownloadManager.DownloadCpp2ILAsync,
                "Cpp2IL").ConfigureAwait(true);

            await LocalService.InstallMelonLoaderAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await MessageBoxService.ErrorAsync("Failed to install MelonLoader: {0}", ex).ConfigureAwait(false);
            Logger.ZLogError(ex, $"Failed to install MelonLoader");
            MelonLoaderInstallStatus = InstallStatus.NotInstalled;
            return;
        }

        Config.MelonLoaderVersion = MelonLoaderVersion;
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

    private async Task CheckAndInstallDotNetRuntimeAsync()
    {
        var runtimeInstalled = await LocalService.CheckDotNetRuntimeInstalledAsync().ConfigureAwait(true);
        if (runtimeInstalled)
        {
            return;
        }

        var result = await MessageBoxService.NoticeAsync(MessageBox_Content_Notice_DotNetRuntime_Install).ConfigureAwait(true);
        if (result is not MessageBoxResult.OK)
        {
            return;
        }

        var success = await PlatformService.InstallDotNetRuntimeAsync().ConfigureAwait(true);
        if (!success)
        {
            await MessageBoxService.ErrorAsync(MessageBox_Content_Error_DotNetRuntime_Install_Failed).ConfigureAwait(false);
        }
    }

    private async Task EnsureValidFileAsync(
        string filePath,
        string expectedHash,
        DownloadFunc downloadFunc,
        string displayName)
    {
        if (File.Exists(filePath))
        {
            var hash = await SHA512Utils.HexFromPathAsync(filePath).ConfigureAwait(false);
            if (hash == expectedHash)
            {
                Logger.ZLogInformation($"{displayName} already exists and hash matches, skipping download");
                return;
            }

            Logger.ZLogInformation($"{displayName} hash mismatch, re-downloading...");
        }
        else
        {
            Logger.ZLogInformation($"{displayName} file does not exist, downloading...");
        }

        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            var success = await downloadFunc(OnDownloadStarted, OnDownloadProgressChanged).ConfigureAwait(false);

            if (success)
            {
                Logger.ZLogInformation($"{displayName} download completed successfully");
                return;
            }

            Logger.ZLogWarning($"Attempt {attempt}/{MaxRetries}: Download of {displayName} failed");
        }

        throw new InvalidOperationException($"Failed to download a valid {displayName} after {MaxRetries} attempts.");
    }

    private void OnDownloadStarted(object? sender, DownloadStartedEventArgs args)
    {
        var fileName = Path.GetFileName(args.FileName);
        var mbSize = args.TotalBytesToReceive / (1024d * 1024d);
        DownloadText = string.Format(XAML.MelonLoader_State_Downloading, fileName, $"{mbSize:F2}");
        Logger.ZLogInformation($"Downloading {fileName}: {args.TotalBytesToReceive}B");
    }

    private void OnDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs args)
    {
        DownloadProgress = args.ProgressPercentage;
    }

    private delegate Task<bool> DownloadFunc(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        EventHandler<DownloadProgressChangedEventArgs> onDownloadProgressChanged,
        CancellationToken cancellationToken = default);

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