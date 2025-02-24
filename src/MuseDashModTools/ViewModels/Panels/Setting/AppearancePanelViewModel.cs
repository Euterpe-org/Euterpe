namespace MuseDashModTools.ViewModels.Panels.Setting;

public sealed class AppearancePanelViewModel : ViewModelBase
{
    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    #endregion Injections
}