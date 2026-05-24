namespace Euterpe.Features.Setting;

public sealed partial class SettingPageViewModel : NavViewModelBase
{
    #region Injections

    public required ILogger<SettingPageViewModel> Logger { get; init; }

    #endregion Injections

    protected override Task OnInitializeAsync()
    {
        Logger.ZLogInformation($"{nameof(SettingPageViewModel)} Initialized");
        return base.OnInitializeAsync();
    }
}