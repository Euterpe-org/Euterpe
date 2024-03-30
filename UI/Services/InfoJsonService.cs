#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace MuseDashModToolsUI.Services;

public sealed partial class InfoJsonService : IInfoJsonService
{
    private InfoJson _infoJson;
    private bool _isInfoJsonInitialized;

    public void Initialize(InfoJson infoJson) => _infoJson = infoJson;

    public async Task OnChooseChartFolderAsync()
    {
        var chartFolder = await FileSystemPickerService.GetSingleFolderPathAsync(FolderDialog_Title_ChooseChartFolder);
        if (string.IsNullOrEmpty(chartFolder))
        {
            Logger.Information("Chosen chart folder is empty");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_InvalidPath);
            return;
        }

        var bmsFiles = LocalService.GetBmsFiles(chartFolder);
        if (bmsFiles is [])
        {
            Logger.Information("Chosen chart folder has no bms file");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_NoBmsFile);
            return;
        }

        foreach (var bmsFile in bmsFiles)
        {
            var match = MapRegex().Match(bmsFile);
            if (!match.Success)
            {
                continue;
            }

            var mapinfo = await ParseMapInfo(bmsFile);
            FillInfoJson(match.Groups[1].Value, mapinfo);
        }
    }

    #region Servies

    [UsedImplicitly]
    public IFileSystemPickerService FileSystemPickerService { get; init; }

    [UsedImplicitly]
    public ILocalService LocalService { get; init; }

    [UsedImplicitly]
    public ILogger Logger { get; init; }

    [UsedImplicitly]
    public IMessageBoxService MessageBoxService { get; init; }

    #endregion
}