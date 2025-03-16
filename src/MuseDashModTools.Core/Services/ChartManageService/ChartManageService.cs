namespace MuseDashModTools.Core;

internal sealed partial class ChartManageService : IChartManageService
{
    private const string ChartAPIUrl = "https://api.mdmc.moe/v2/charts/";
    private SourceCache<Chart, string> _sourceCache = null!;

    public async Task InitializeChartsAsync(SourceCache<Chart, string> sourceCache)
    {
        _sourceCache = sourceCache;

        await foreach (var chart in GetChartListAsync())
        {
            if (chart is null)
            {
                continue;
            }

            _sourceCache.AddOrUpdate(chart);
        }
    }

    #region Injections

    [UsedImplicitly]
    public required HttpClient Client { get; init; }

    [UsedImplicitly]
    public required ILogger<ChartManageService> Logger { get; init; }

    #endregion Injections
}