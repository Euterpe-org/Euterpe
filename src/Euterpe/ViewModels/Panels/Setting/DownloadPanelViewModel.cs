namespace Euterpe.ViewModels.Panels.Setting;

public sealed partial class DownloadPanelViewModel : ViewModelBase
{
    public static IReadOnlyList<LocalizedString> UpdateChannels { get; } =
    [
        Setting_UpdateChannel_Stable,
        Setting_UpdateChannel_Prerelease
    ];

    [ObservableProperty]
    public partial int SelectedUpdateChannelIndex { get; set; }

    public override Task InitializeAsync()
    {
        base.InitializeAsync();

        Logger.ZLogInformation($"{nameof(DownloadPanelViewModel)} Initialized");
        return Task.CompletedTask;
    }

    partial void OnSelectedUpdateChannelIndexChanged(int value) => Config.UpdateChannel = (UpdateChannel)value;

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required ILogger<DownloadPanelViewModel> Logger { get; init; }

    #endregion Injections
}