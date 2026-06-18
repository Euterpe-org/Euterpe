namespace Euterpe.Features.Setting;

[Route("/setting/download", DisplayName = Panel_Setting_Download, Order = 4)]
[AppSingleton]
public sealed partial class DownloadPanelViewModel : ViewModelBase
{
    public static IReadOnlyList<LocalizedString> UpdateChannels { get; } =
    [
        Setting_UpdateChannel_Stable,
        Setting_UpdateChannel_Prerelease
    ];

    [ObservableProperty]
    public partial int SelectedUpdateChannelIndex { get; set; }

    protected override Task OnInitializeAsync()
    {
        Logger.ZLogInformation($"{nameof(DownloadPanelViewModel)} Initialized");
        return base.OnInitializeAsync();
    }

    partial void OnSelectedUpdateChannelIndexChanged(int value) => Config.UpdateChannel = (UpdateChannel)value;

    #region Injections

    public required Config Config { get; init; }
    public required ILogger<DownloadPanelViewModel> Logger { get; init; }

    #endregion Injections
}
