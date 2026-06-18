namespace Euterpe.Features.Setting;

[Route("/setting/experience", DisplayName = Panel_Setting_Experience, Order = 2)]
[AppSingleton]
public sealed class ExperiencePanelViewModel : ViewModelBase
{
    #region Injections

    public required Config Config { get; init; }

    #endregion Injections
}
