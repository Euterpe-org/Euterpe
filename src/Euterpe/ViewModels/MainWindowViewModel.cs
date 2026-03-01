namespace Euterpe.ViewModels;

public sealed partial class MainWindowViewModel : NavViewModelBase
{
    public override IReadOnlyList<NavItem> NavItems { get; } =
    [
        new(Page_Home, HomePageName, "Home"),
        new(Page_Modding, ModdingPageName, "Wrench"),
        new(Page_Charting, ChartingPageName, "Music"),
        new(Page_Logging, LoggingPageName, "Terminal"),
        new(Page_Setting, SettingPageName, "Setting")
    ];

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync().ConfigureAwait(true);

        await SettingService.ValidateAsync().ConfigureAwait(true);
#if RELEASE
        await UpdateService.CheckForUpdatesAsync().ConfigureAwait(true);
#endif
        await LocalService.ReadGameInformationAsync().ConfigureAwait(false);
        LocalService.ReadMelonLoaderVersion();

        await CheckAndInstallDotNetRuntimeAsync().ConfigureAwait(false);

        Logger.ZLogInformation($"{nameof(MainWindowViewModel)} Initialized");
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
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required ILocalService LocalService { get; init; }

    [UsedImplicitly]
    public required LocalizationService LocalizationService { get; init; }

    [UsedImplicitly]
    public required ILogger<MainWindowViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }
#if RELEASE
    [UsedImplicitly]
    public required IUpdateService UpdateService { get; init; }
#endif
    [UsedImplicitly]
    public required ISettingService SettingService { get; init; }

    #endregion Injections
}