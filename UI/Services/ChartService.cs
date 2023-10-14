using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using DynamicData;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace MuseDashModToolsUI.Services;

public partial class ChartService : IChartService
{
    private ReadOnlyObservableCollection<Chart> _charts;
    private SourceCache<Chart, string> _sourceCache = new(x => x.Name);

    [UsedImplicitly]
    public IFileSystemPickerService FileSystemPickerService { get; init; }

    [UsedImplicitly]
    public IGitHubService GitHubService { get; init; }

    [UsedImplicitly]
    public ILocalService LocalService { get; init; }

    [UsedImplicitly]
    public IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public ISavingService SavingService { get; init; }

    public async Task DownloadChart(Chart item)
    {
        var path = Path.Combine(SavingService.Settings.CustomAlbumsFolder, item.Name.RemoveInvalidChars() + ".mdm");
        if (File.Exists(path))
        {
            var result = await MessageBoxService.FormatNoticeConfirmMessageBox(MsgBox_Content_OverrideChart, item.Name);
            if (!result) return;
        }

        await GitHubService.DownloadChart(item.Id, path);
        await MessageBoxService.FormatSuccessMessageBox(MsgBox_Content_DownloadChartSuccess, item.Name);
    }

    public async Task InitializeChartList(SourceCache<Chart, string> sourceCache, ReadOnlyObservableCollection<Chart> charts)
    {
        _sourceCache = sourceCache;
        _charts = charts;

        if (!Directory.Exists(SavingService.Settings.CustomAlbumsFolder))
            Directory.CreateDirectory(SavingService.Settings.CustomAlbumsFolder);

        var webCharts = await GitHubService.GetChartList();
        _sourceCache.AddOrUpdate(webCharts!);
    }

    public async Task OnChooseChartFolder()
    {
        var chartFolder = await FileSystemPickerService.GetSingleFolderPath(FolderDialog_Title_ChooseChartFolder);
        if (chartFolder is null)
        {
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_InvalidPath);
            return;
        }

        var bmsFiles = LocalService.GetBmsFiles(chartFolder).ToArray();
        if (bmsFiles.Length == 0)
        {
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_NoBmsFile);
            return;
        }

        foreach (var bmsFile in bmsFiles)
        {
            if (MapRegex().Match(bmsFile).Success)
            {
            }
        }
    }

    [GeneratedRegex("map([1-4]).bms")]
    private static partial Regex MapRegex();
}