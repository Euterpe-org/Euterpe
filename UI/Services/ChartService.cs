using System.Collections.ObjectModel;
using DynamicData;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace MuseDashModToolsUI.Services;

public class ChartService : IChartService
{
    private ReadOnlyObservableCollection<Chart> _charts;
    private SourceCache<Chart, string> _sourceCache = new(x => x.Name);

    [UsedImplicitly]
    public IGitHubService GitHubService { get; init; }

    public async Task InitializeChartList(SourceCache<Chart, string> sourceCache, ReadOnlyObservableCollection<Chart> charts)
    {
        _sourceCache = sourceCache;
        _charts = charts;

        var webCharts = await GitHubService.GetChartList();
        _sourceCache.AddOrUpdate(webCharts);
    }
}