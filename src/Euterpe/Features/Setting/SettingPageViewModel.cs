namespace Euterpe.Features.Setting;

[Route("/setting", DisplayName = Page_Setting, Icon = "Setting", Order = 4)]
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
