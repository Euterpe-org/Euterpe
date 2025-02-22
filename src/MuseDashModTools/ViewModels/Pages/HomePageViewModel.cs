using Ursa.Common;
using Ursa.Controls.Options;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class HomePageViewModel : ViewModelBase
{
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
    private void LaunchModdedGame()
    {
        // TODO
    }

    [RelayCommand]
    private void LaunchVanillaGame()
    {
        // TODO
    }

    #region Injections

    [UsedImplicitly]
    public DonationDialogViewModel DonationDialogViewModel { get; init; } = null!;

    [UsedImplicitly]
    public ILogger<HomePageViewModel> Logger { get; init; } = null!;

    #endregion Injections
}