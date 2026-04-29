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

    protected override Task OnInitializeAsync()
    {
        SelectedGameModeIndex = (int)Config.GameMode;
        return base.OnInitializeAsync();
    }

    [RelayCommand]
    private Task LaunchGameAsync()
    {
        return Config.GameMode switch
        {
            GameMode.Modded => GameService.LaunchModdedGameAsync(),
            GameMode.Vanilla => GameService.LaunchVanillaGameAsync(),
            _ => throw new UnreachableException()
        };
    }

    partial void OnSelectedGameModeIndexChanged(int value) => Config.GameMode = (GameMode)value;

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required IGameService GameService { get; init; }

    [UsedImplicitly]
    public required ILogger<HomePageViewModel> Logger { get; init; }

    #endregion Injections
}