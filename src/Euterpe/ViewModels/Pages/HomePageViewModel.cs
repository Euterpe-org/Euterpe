using Euterpe.Core.Http.Clients;

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
            await ShowFullWizardAsync().ConfigureAwait(true);
        }
        else if (!GameSettingService.IsValidGameFolder())
        {
            Logger.ZLogWarning($"Stored {GameConfig.DisplayName} folder is invalid, opening game path repair");
            await ShowGamePathRepairAsync().ConfigureAwait(true);
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

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required GameSwitcher GameSwitcher { get; init; }

    [UsedImplicitly]
    public required NavigationService NavigationService { get; init; }

    [UsedImplicitly]
    public required WizardDialogViewModel WizardDialogViewModel { get; init; }

    [UsedImplicitly]
    public required RepairDialogViewModel RepairDialogViewModel { get; init; }

    [UsedImplicitly]
    public required IDialogService DialogService { get; init; }

    [UsedImplicitly]
    public required IEuterpeAccountClient AccountClient { get; init; }

    [UsedImplicitly]
    public required IGameLaunchService GameLaunchService { get; init; }

    [UsedImplicitly]
    public required IGameSettingService GameSettingService { get; init; }

    [UsedImplicitly]
    public required IGameLocalService GameLocalService { get; init; }

    [UsedImplicitly]
    public required ILogger<HomePageViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public required IGameRuntimeInstaller RuntimeInstaller { get; init; }

    [UsedImplicitly]
    public required IGameUidProvider UidProvider { get; init; }

    #endregion Injections
}