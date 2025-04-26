namespace MuseDashModTools.ViewModels.Panels.Setting;

public sealed class AdvancedPanelViewModel : ViewModelBase
{
    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    #endregion Injections
}