using System.Collections.ObjectModel;
using System.IO;
using DynamicData;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace MuseDashModToolsUI.Services;

public sealed class ChartService : IChartService
{
    private ReadOnlyObservableCollection<Chart> _charts;
    private SourceCache<Chart, string> _sourceCache;

    [UsedImplicitly]
    public IGitHubService GitHubService { get; init; }

    [UsedImplicitly]
    public ILogger Logger { get; init; }

    [UsedImplicitly]
    public IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public ISavingService SavingService { get; init; }

    public async Task DownloadChartAsync(Chart item)
    {
        var path = Path.Combine(SavingService.Settings.CustomAlbumsFolder, item.Name.RemoveInvalidChars() + ".mdm");
        if (File.Exists(path))
        {
            var result = await MessageBoxService.FormatNoticeConfirmMessageBox(MsgBox_Content_OverrideChart, item.Name);
            if (!result)
            {
                return;
            }
        }

        await GitHubService.DownloadChartAsync(item.Id, path);
        await MessageBoxService.FormatSuccessMessageBox(MsgBox_Content_DownloadChartSuccess, item.Name);
    }

    public async Task InitializeChartList(SourceCache<Chart, string> sourceCache, ReadOnlyObservableCollection<Chart> charts)
    {
        _sourceCache = sourceCache;
        _charts = charts;

        if (!Directory.Exists(SavingService.Settings.CustomAlbumsFolder))
        {
            Directory.CreateDirectory(SavingService.Settings.CustomAlbumsFolder);
        }

        var webCharts = await GitHubService.GetChartListAsync();
        _sourceCache.AddOrUpdate(webCharts!);
    }
}