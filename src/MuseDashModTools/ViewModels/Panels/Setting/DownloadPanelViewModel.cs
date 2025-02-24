namespace MuseDashModTools.ViewModels.Panels.Setting;

public sealed partial class DownloadPanelViewModel : ViewModelBase
{
    public static string[] DownloadSources { get; } =
    [
        XAML_DownloadSource_Github,
        XAML_DownloadSource_GithubMirror,
        XAML_DownloadSource_Custom
    ];

    public static string[] UpdateSources { get; } =
    [
        XAML_UpdateSources_GithubAPI,
        XAML_UpdateSources_GithubRSS
    ];

    [ObservableProperty]
    public partial int SelectedDownloadSourceIndex { get; set; }

    [ObservableProperty]
    public partial int SelectedUpdateSourceIndex { get; set; }

    protected override void Initialize()
    {
        SelectedDownloadSourceIndex = (int)Config.DownloadSource;
        SelectedUpdateSourceIndex = (int)Config.UpdateSource;
    }

    [UsedImplicitly]
    partial void OnSelectedDownloadSourceIndexChanged(int value) => Config.DownloadSource = (DownloadSource)value;

    [UsedImplicitly]
    partial void OnSelectedUpdateSourceIndexChanged(int value) => Config.UpdateSource = (UpdateSource)value;

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required ILogger<DownloadPanelViewModel> Logger { get; init; }

    #endregion Injections
}