using Ursa.Common;
using Ursa.Controls.Options;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class HomePageViewModel : ViewModelBase
{
    public IReadOnlyList<LocalizedString> GameModes { get; } =
    [
        Dropdown_Modded,
        Dropdown_Vanilla
    ];

    [ObservableProperty]
    public partial int SelectedGameModeIndex { get; set; }

    public override Task InitializeAsync()
    {
        SelectedGameModeIndex = (int)Config.GameMode;
        return base.InitializeAsync();
    }

    [RelayCommand]
    private Task<DialogResult> ShowDonationDrawerAsync()
    {
        var options = new DrawerOptions
        {
            Position = Position.Right,
            Title = "原神启动",
            IsCloseButtonVisible = true,
            CanLightDismiss = true,
            MaxWidth = 700,
            MinWidth = 700,
            Buttons = DialogButton.None,
            CanResize = false
        };

        return Drawer.ShowModal<DonationDialog, DonationDialogViewModel>(DonationDialogViewModel, "DonationDrawerHost", options);
    }

    [RelayCommand]
    private void LaunchGame()
    {
        switch (Enum.GetValues<GameMode>()[SelectedGameModeIndex])
        {
            case GameMode.Modded:
                GameService.LaunchModdedGame();
                break;
            case GameMode.Vanilla:
                GameService.LaunchVanillaGame();
                break;
            default:
                throw new UnreachableException();
        }
    }

    [UsedImplicitly]
    partial void OnSelectedGameModeIndexChanged(int value) => Config.GameMode = (GameMode)value;

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required DonationDialogViewModel DonationDialogViewModel { get; init; }

    [UsedImplicitly]
    public required IGameService GameService { get; init; }

    [UsedImplicitly]
    public required ILogger<HomePageViewModel> Logger { get; init; }

    #endregion Injections
}