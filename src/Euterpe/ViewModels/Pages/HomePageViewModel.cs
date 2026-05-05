using Euterpe.Core.Http.Clients;
using Semver;

namespace Euterpe.ViewModels.Pages;

public sealed partial class HomePageViewModel : ViewModelBase
{
    public IReadOnlyList<LocalizedString> GameModes { get; } =
    [
        Dropdown_Modded,
        Dropdown_Vanilla
    ];

    [ObservableProperty]
    public partial int SelectedGameModeIndex { get; set; }

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(true);
        await NavigationService.Ready.WaitAsync().ConfigureAwait(true);

        await GameSettingService.ValidateGameAsync().ConfigureAwait(true);
        await LocalService.ReadGameInformationAsync().ConfigureAwait(true);
        LocalService.ReadMelonLoaderVersion();

        SelectedGameModeIndex = (int)GameConfig.GameMode;

        BindAccountAsync().SafeFireAndForget();
        CheckModdingDependenciesAsync().SafeFireAndForget(ex => Logger.ZLogError(ex, $"Failed to check modding dependencies"));

        Logger.ZLogInformation($"{nameof(HomePageViewModel)} Initialized");
    }

    private async Task CheckModdingDependenciesAsync()
    {
        await CheckDotNetRuntimeAsync().ConfigureAwait(true);
        await CheckMelonLoaderAsync().ConfigureAwait(true);
    }

    private async Task CheckDotNetRuntimeAsync()
    {
        if (await RuntimeInstaller.CheckInstalledAsync().ConfigureAwait(true))
        {
            return;
        }

        var result = await MessageBoxService.NoticeAsync(MessageBox_Content_DotNetRuntime_Install).ConfigureAwait(true);
        if (result is not MessageBoxResult.OK)
        {
            return;
        }

        var success = await RuntimeInstaller.InstallAsync().ConfigureAwait(true);
        if (!success)
        {
            await MessageBoxService.ErrorAsync(MessageBox_Content_DotNetRuntime_Install_Failed).ConfigureAwait(false);
        }
    }

    [RelayCommand]
    private Task LaunchGameAsync()
    {
        return GameConfig.GameMode switch
        {
            GameMode.Modded => GameLaunchService.LaunchModdedGameAsync(),
            GameMode.Vanilla => GameLaunchService.LaunchVanillaGameAsync(),
            _ => throw new UnreachableException()
        };
    }

    partial void OnSelectedGameModeIndexChanged(int value) => GameConfig.GameMode = (GameMode)value;

    private async Task BindAccountAsync()
    {
        var request = await UidProvider.GetMuseDashUidRequestAsync().ConfigureAwait(false);
        if (request is null)
        {
            Logger.ZLogWarning($"Failed to get MuseDash user ID. Skipping account binding.");
            return;
        }

        try
        {
            await AccountClient.BindVanillaAccountAsync(request).ConfigureAwait(false);
            Logger.ZLogInformation($"Successfully bound MuseDash account.");
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to bind MuseDash account.");
        }
    }

    private async Task CheckMelonLoaderAsync()
    {
        if (GameConfig.MelonLoaderSemVersion is not { } localVersion)
        {
            Logger.ZLogInformation($"MelonLoader not installed, prompting user");

            await MessageBoxService.NoticeAsync(MessageBox_Content_MelonLoader_NotInstalled).ConfigureAwait(true);
            await NavigationService.NavigateToAsync("/modding/melonloader").ConfigureAwait(false);
            return;
        }

        var version = await DependencyAcquireService.GetLatestMelonLoaderVersionAsync().ConfigureAwait(true);
        if (!SemVersion.TryParse(version, out var latestVersion))
        {
            Logger.ZLogWarning($"Failed to parse MelonLoader version {version}");
            return;
        }

        if (localVersion.ComparePrecedenceTo(latestVersion) >= 0)
        {
            return;
        }

        Logger.ZLogInformation($"MelonLoader outdated: {localVersion} < {latestVersion}, prompting user");

        await MessageBoxService.NoticeAsync(MessageBox_Content_MelonLoader_Outdated, localVersion, latestVersion).ConfigureAwait(true);
        await NavigationService.NavigateToAsync("/modding/melonloader").ConfigureAwait(true);
    }

    #region Injections

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required IEuterpeAccountClient AccountClient { get; init; }

    [UsedImplicitly]
    public required IDependencyAcquireService DependencyAcquireService { get; init; }

    [UsedImplicitly]
    public required IGameLaunchService GameLaunchService { get; init; }

    [UsedImplicitly]
    public required IGameSettingService GameSettingService { get; init; }

    [UsedImplicitly]
    public required ILocalService LocalService { get; init; }

    [UsedImplicitly]
    public required IGameRuntimeInstaller RuntimeInstaller { get; init; }

    [UsedImplicitly]
    public required IGameUidProvider UidProvider { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public required NavigationService NavigationService { get; init; }

    [UsedImplicitly]
    public required ILogger<HomePageViewModel> Logger { get; init; }

    #endregion Injections
}