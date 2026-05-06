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

        if (GameConfig.SetupCompleted)
        {
            await GameSettingService.ValidateGameFolderAsync().ConfigureAwait(true);
        }
        else
        {
            await ShowWizardDialogAsync().ConfigureAwait(true);
        }

        GameSettingService.EnsureGameFolders();

        await GameLocalService.ReadGameInformationAsync().ConfigureAwait(true);
        GameLocalService.ReadMelonLoaderVersion();

        SelectedGameModeIndex = (int)GameConfig.GameMode;

        BindAccountAsync().SafeFireAndForget();
        CheckModdingDependenciesAsync().SafeFireAndForget(ex => Logger.ZLogError(ex, $"Failed to check modding dependencies"));

        Logger.ZLogInformation($"{nameof(HomePageViewModel)} Initialized");
    }

    private async Task ShowWizardDialogAsync(WizardOptionKinds? singleOption = null)
    {
        Logger.ZLogInformation($"Showing setup wizard dialog (singleOption={singleOption})");

        await WizardDialogViewModel.PrepareForShowAsync(singleOption).ConfigureAwait(true);

        var options = new OverlayDialogOptions
        {
            Title = singleOption is null ? Wizard_Title_Welcome : Wizard_Title_SettingUp,
            CanDragMove = false,
            CanResize = false,
            IsCloseButtonVisible = false
        };

        GameSwitcher.CanSwitch = false;
        try
        {
            await OverlayDialog.ShowCustomAsync<WizardDialog, WizardDialogViewModel, object>(WizardDialogViewModel, MainWindowViewModel.WizardHostId, options).ConfigureAwait(true);
        }
        finally
        {
            GameSwitcher.CanSwitch = true;
        }
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
        if (GameConfig.MelonLoaderSemVersion is not null)
        {
            return;
        }

        Logger.ZLogInformation($"MelonLoader not installed, opening single-step wizard");
        await ShowWizardDialogAsync(WizardOptionKinds.MelonLoader).ConfigureAwait(false);
    }

    #region Injections

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required IEuterpeAccountClient AccountClient { get; init; }

    [UsedImplicitly]
    public required IGameLaunchService GameLaunchService { get; init; }

    [UsedImplicitly]
    public required IGameSettingService GameSettingService { get; init; }

    [UsedImplicitly]
    public required IGameLocalService GameLocalService { get; init; }

    [UsedImplicitly]
    public required IGameRuntimeInstaller RuntimeInstaller { get; init; }

    [UsedImplicitly]
    public required IGameUidProvider UidProvider { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public required NavigationService NavigationService { get; init; }

    [UsedImplicitly]
    public required GameSwitcher GameSwitcher { get; init; }

    [UsedImplicitly]
    public required WizardDialogViewModel WizardDialogViewModel { get; init; }

    [UsedImplicitly]
    public required ILogger<HomePageViewModel> Logger { get; init; }

    #endregion Injections
}