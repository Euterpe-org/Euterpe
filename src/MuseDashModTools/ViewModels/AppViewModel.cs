namespace MuseDashModTools.ViewModels;

public sealed class AppViewModel : ViewModelBase
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync().ConfigureAwait(false);

        await SettingService.LoadAsync().ConfigureAwait(false);

        Logger.ZLogInformation($"{nameof(AppViewModel)} Initialized");
    }

    #region Injections

    [UsedImplicitly]
    public required ILogger<AppViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required ISettingService SettingService { get; init; }

    #endregion Injections
}