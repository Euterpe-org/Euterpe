namespace Euterpe.ViewModels.Windows;

public sealed partial class MainWindowViewModel : NavViewModelBase
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync().ConfigureAwait(true);

        await SettingService.ValidateAsync().ConfigureAwait(true);
        await LocalService.ReadGameInformationAsync().ConfigureAwait(false);
        LocalService.ReadMelonLoaderVersion();

        NavigationService.Ready.Set();
        Logger.ZLogInformation($"{nameof(MainWindowViewModel)} Initialized");
    }


    #region Injections

    [UsedImplicitly]
    public required ISettingService SettingService { get; init; }

    [UsedImplicitly]
    public required ILocalService LocalService { get; init; }

    [UsedImplicitly]
    public required ILogger<MainWindowViewModel> Logger { get; init; }

    #endregion Injections
}