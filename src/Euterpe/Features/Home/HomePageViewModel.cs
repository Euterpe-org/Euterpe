using Euterpe.Core.Http.Clients;

namespace Euterpe.Features.Home;

[Route("/home", DisplayName = Page_Home, Icon = "Home", Order = 0)]
[PerGame]
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

        await EnsureSetupAsync().ConfigureAwait(true);
        await LoadGameStateAsync().ConfigureAwait(true);
        StartBackgroundTasks();

        Logger.ZLogInformation($"{nameof(HomePageViewModel)} Initialized");
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

    private async Task EnsureSetupAsync()
    {
        if (!GameConfig.SetupCompleted)
        {
            Logger.ZLogInformation($"Setup not completed, opening full setup wizard");
            await SetupDialogService.ShowFullWizardAsync().ConfigureAwait(true);
        }
        else if (!GameSettingService.IsValidGameFolder())
        {
            Logger.ZLogWarning($"Stored {GameConfig.DisplayName} folder is invalid, opening game path repair");
            await SetupDialogService.ShowGamePathRepairAsync().ConfigureAwait(true);
        }
    }

    private async Task LoadGameStateAsync()
    {
        GameSettingService.EnsureGameFolders();

        try
        {
            GameLocalService.ReadGameInformation();
        }
        catch (Exception ex)
        {
            Logger.ZLogCritical(ex, $"Read game information failed; aborting startup");
            await MessageBoxService.ErrorAsync(MessageBox_Content_ReadGameInformation_Failed, GameConfig.GlobalGameManagersPath).ConfigureAwait(true);
            Environment.Exit(1);
            return;
        }

        GameLocalService.ReadMelonLoaderVersion();
        SelectedGameModeIndex = (int)GameConfig.GameMode;
    }

    partial void OnSelectedGameModeIndexChanged(int value) => GameConfig.GameMode = (GameMode)value;

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required NavigationService NavigationService { get; init; }
    public required SetupDialogService SetupDialogService { get; init; }
    public required IEuterpeAccountClient AccountClient { get; init; }
    public required IChartManageService ChartManageService { get; init; }
    public required IGameLaunchService GameLaunchService { get; init; }
    public required IGameSettingService GameSettingService { get; init; }
    public required IGameLocalService GameLocalService { get; init; }
    public required ILogger<HomePageViewModel> Logger { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }
    public required IGameRuntimeInstaller RuntimeInstaller { get; init; }
    public required IGameUidProvider UidProvider { get; init; }

    #endregion Injections
}