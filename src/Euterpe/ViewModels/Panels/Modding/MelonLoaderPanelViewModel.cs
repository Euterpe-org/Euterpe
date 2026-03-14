using static Euterpe.Shared.DependencyConstants;

namespace Euterpe.ViewModels.Panels.Modding;

public sealed partial class MelonLoaderPanelViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial InstallStatus MelonLoaderInstallStatus { get; set; }

    [ObservableProperty]
    public partial LocalizedString? DownloadText { get; set; }

    [ObservableProperty]
    public partial double DownloadProgress { get; set; }

    private DependencySpec[] GetMelonLoaderDependencies() =>
    [
        new("MelonLoader", MelonLoader.Url, Config.MelonLoaderZipPath, MelonLoader.ZipHash),
        new("UnityDependency", UnityRuntime.Url, Config.UnityDependencyZipPath, UnityRuntime.ZipHash),
        new("Cpp2IL", Cpp2IL.ExecutableUrl, Config.Cpp2ILExecutablePath, Cpp2IL.ExecutableHash),
        new("Cpp2IL Plugin", Cpp2IL.PluginUrl, Config.Cpp2ILPluginPath, Cpp2IL.PluginHash)
    ];

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
            foreach (var dependency in GetMelonLoaderDependencies())
            {
                await DependencyAcquireService.EnsureValidAsync(dependency, OnDownloadStarted, progress).ConfigureAwait(true);
            }

            await LocalService.InstallMelonLoaderAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await MessageBoxService.ErrorAsync(MessageBox_Content_MelonLoader_Install_Failed, ex).ConfigureAwait(false);
            Logger.ZLogError(ex, $"Failed to install MelonLoader");
            MelonLoaderInstallStatus = InstallStatus.NotInstalled;
            return;
        }

        Config.MelonLoaderVersion = MelonLoader.Version;
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