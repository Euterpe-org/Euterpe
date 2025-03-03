namespace MuseDashModTools.ViewModels.Panels.Setting;

public sealed partial class DownloadPanelViewModel : ViewModelBase
{
    public static IReadOnlyList<string> DownloadSources { get; } =
    [
        Setting_DownloadSource_GitHub,
        Setting_DownloadSource_GitHubMirror,
        Setting_DownloadSource_Gitee,
        Setting_DownloadSource_Custom
    ];

    public static IReadOnlyList<string> UpdateSources { get; } =
    [
        Setting_UpdateSources_GitHubAPI,
        Setting_UpdateSources_GitHubRSS
    ];

    [ObservableProperty]
    public partial int SelectedDownloadSourceIndex { get; set; }

    [ObservableProperty]
    public partial int SelectedUpdateSourceIndex { get; set; }

    [RelayCommand]
    protected override Task InitializeAsync()
    {
        base.InitializeAsync();

        SelectedDownloadSourceIndex = (int)Config.DownloadSource;
        SelectedUpdateSourceIndex = (int)Config.UpdateSource;

        Logger.ZLogInformation($"{nameof(DownloadPanelViewModel)} Initialized");
        return Task.CompletedTask;
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