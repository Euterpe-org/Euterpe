namespace Euterpe.ViewModels.Pages;

public sealed partial class SettingPageViewModel : NavViewModelBase
{
    #region Injections

    [UsedImplicitly]
    public required ILogger<SettingPageViewModel> Logger { get; init; }

    #endregion Injections

    public override Task InitializeAsync()
    {
        base.InitializeAsync();

        Logger.ZLogInformation($"{nameof(SettingPageViewModel)} Initialized");
        return Task.CompletedTask;
    }
}