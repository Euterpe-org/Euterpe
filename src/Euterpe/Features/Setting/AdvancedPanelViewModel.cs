namespace Euterpe.Features.Setting;

[Route("/setting/advanced", DisplayName = Panel_Setting_Advanced, Order = 5)]
[AppSingleton]
public sealed class AdvancedPanelViewModel : ViewModelBase
{
    #region Injections

    public required Config Config { get; init; }

    #endregion Injections
}
