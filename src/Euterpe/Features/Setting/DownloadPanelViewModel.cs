namespace Euterpe.Features.Setting;

[Route("/setting/download", DisplayName = Panel_Setting_Download, Order = 4)]
[AppSingleton]
public sealed partial class DownloadPanelViewModel : ViewModelBase
{
    public static IReadOnlyList<EnumOption<UpdateChannel>> UpdateChannels { get; } =
    [
        .. UpdateChannelExtensions.GetValues().Select(static channel =>
            new EnumOption<UpdateChannel>(channel, $"{nameof(UpdateChannel)}_{channel.ToStringFast()}"))
    ];

    protected override Task OnInitializeAsync()
    {
        Logger.LogInformation("{ViewModel} Initialized", nameof(DownloadPanelViewModel));
        return base.OnInitializeAsync();
    }

    #region Injections

    public required Config Config { get; init; }
    public required ILogger<DownloadPanelViewModel> Logger { get; init; }

    #endregion Injections
}
