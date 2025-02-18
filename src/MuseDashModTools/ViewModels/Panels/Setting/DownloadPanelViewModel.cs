namespace MuseDashModTools.ViewModels.Panels.Setting;

public sealed class DownloadPanelViewModel : ViewModelBase
{
    public IEnumerable<DownloadSource> DownloadSources =>
        Enum.GetValues<DownloadSource>();

    public IEnumerable<UpdateSource> UpdateSources =>
        Enum.GetValues<UpdateSource>();

    #region Injections

    [UsedImplicitly]
    public Config Config { get; init; } = null!;

    #endregion Injections
}