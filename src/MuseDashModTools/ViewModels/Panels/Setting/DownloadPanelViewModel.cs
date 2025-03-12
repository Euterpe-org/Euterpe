namespace MuseDashModTools.ViewModels.Panels.Setting;

public sealed partial class DownloadPanelViewModel : ViewModelBase
{
    public static IReadOnlyList<LocalizedString> DownloadSources { get; } =
    [
        Setting_DownloadSource_GitHubLiteral,
        Setting_DownloadSource_GitHubMirrorLiteral,
        Setting_DownloadSource_GiteeLiteral,
        Setting_DownloadSource_CustomLiteral
    ];

    public static IReadOnlyList<LocalizedString> UpdateSources { get; } =
    [
        Setting_UpdateSources_GitHubAPILiteral,
        Setting_UpdateSources_GitHubRSSLiteral
    ];

    [ObservableProperty]
    public partial int SelectedDownloadSourceIndex { get; set; }

    [ObservableProperty]
    public partial int SelectedUpdateSourceIndex { get; set; }

    public override Task InitializeAsync()
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