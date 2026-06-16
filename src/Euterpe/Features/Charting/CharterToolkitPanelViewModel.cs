namespace Euterpe.Features.Charting;

[Route("/charting/toolkit", DisplayName = Panel_Charting_CharterToolkit, Order = 1)]
[PerGame]
public sealed partial class CharterToolkitPanelViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial ViewModelBase? ActiveTool { get; set; }

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        Editor.CloseRequested += OnEditorCloseRequested;
        Editor.Saved += OnEditorSaved;

        Logger.ZLogInformation($"{nameof(CharterToolkitPanelViewModel)} Initialized");
    }

    [RelayCommand]
    private async Task OpenEpkEditorAsync()
    {
        if (await FileSystemPickerService.GetSingleFilePathAsync(FileDialog_Title_ChooseEpkFile).ConfigureAwait(true) is { } filePath)
        {
            await OpenEpkAsync(filePath).ConfigureAwait(true);
        }
    }

    public async Task OpenEpkAsync(string filePath)
    {
        Manifest manifest;
        try
        {
            manifest = await MessagePackSerialization.DeserializeManifestFromFileAsync(filePath).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to read EPK file {filePath}");
            NotificationService.ErrorLight(Notification_Content_Epk_Open_Invalid);
            return;
        }

        if (manifest.Cid is not null)
        {
            NotificationService.NoticeLight(Notification_Content_Epk_Edit_OnlineReadonly);
            return;
        }

        Editor.Open(filePath, manifest);
        ActiveTool = Editor;
    }

    private void OnEditorCloseRequested() => ActiveTool = null;

    private void OnEditorSaved(string folderPath) =>
        ChartManageService.RefreshChartAsync(folderPath).SafeFireAndForget(ex => Logger.ZLogError(ex, $"Failed to refresh chart after editing {folderPath}"));

    #region Injections

    public required IChartManageService ChartManageService { get; init; }
    public required EpkEditorPanelViewModel Editor { get; init; }
    public required IFileSystemPickerService FileSystemPickerService { get; init; }
    public required ILogger<CharterToolkitPanelViewModel> Logger { get; init; }
    public required IMessagePackSerializationService MessagePackSerialization { get; init; }
    public required INotificationService NotificationService { get; init; }

    #endregion Injections
}
