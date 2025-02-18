namespace MuseDashModTools.ViewModels.Panels.Setting;

public sealed partial class DownloadPanelViewModel : ViewModelBase
{
    public static string[] DownloadSources { get; } =
    [
        XAML_DownloadSource_Github,
        XAML_DownloadSource_GithubMirror,
        XAML_DownloadSource_Custom,
    ];
    public static string[] UpdateSources { get; } =
    [
        XAML_UpdateSources_GithubAPI,
        XAML_UpdateSources_GithubRSS,
    ];

    [ObservableProperty]
    public partial int SelectedDownloadSourceIndex { get; set; }

    [ObservableProperty]
    public partial int SelectedUpdateSourceIndex { get; set; }

    [RelayCommand]
    private Task InitializeAsync()
    {
        SelectedDownloadSourceIndex = (int)Config.DownloadSource;
        SelectedUpdateSourceIndex = (int)Config.UpdateSource;
        return Task.CompletedTask;
    }

    partial void OnSelectedDownloadSourceIndexChanged(int value) => Config.DownloadSource = (DownloadSource)value;

    partial void OnSelectedUpdateSourceIndexChanged(int value) => Config.UpdateSource = (UpdateSource)value;

    #region Injections

    [UsedImplicitly]
    public Config Config { get; init; } = null!;

    #endregion Injections
}