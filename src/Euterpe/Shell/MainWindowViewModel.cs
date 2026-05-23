namespace Euterpe.Shell;

public sealed partial class MainWindowViewModel : NavViewModelBase
{
    public const string DialogHostId = "DialogHost";

    [ObservableProperty]
    public partial GameConfig SelectedGame { get; set; } = null!;

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(true);

        SelectedGame = Config.ActiveGameConfig;
        await AppSettingService.ValidateSteamAsync().ConfigureAwait(true);

        NavigationService.Ready.Set();

        Logger.ZLogInformation($"{nameof(MainWindowViewModel)} Initialized");
    }

    partial void OnSelectedGameChanged(GameConfig value) =>
        GameSwitcher.SwitchAsync(value.Id).SafeFireAndForget(ex => Logger.ZLogError(ex, $"Failed to switch game to {value.Id}"));

    #region Injections

    [UsedImplicitly]
    public required AuthState AuthState { get; init; }

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required GameSwitcher GameSwitcher { get; init; }

    [UsedImplicitly]
    public required IAppSettingService AppSettingService { get; init; }

    [UsedImplicitly]
    public required ILogger<MainWindowViewModel> Logger { get; init; }

    #endregion Injections
}