namespace MuseDashModTools.ViewModels.Panels.Setting;

public sealed partial class ExperiencePanelViewModel : ViewModelBase
{
    #region Injections

    [UsedImplicitly]
    public Config Config { get; init; } = null!;

    #endregion Injections
}