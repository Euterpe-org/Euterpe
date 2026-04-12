namespace Euterpe.Core;

internal sealed partial class ChartManageService : IChartManageService
{
    private SourceCache<Chart, string> _sourceCache = null!;

    public async Task InitializeChartsAsync(SourceCache<Chart, string> sourceCache)
    {
        _sourceCache = sourceCache;
    }

    #region Injections

    [UsedImplicitly]
    public required HttpClient Client { get; init; }

    [UsedImplicitly]
    public required ILogger<ChartManageService> Logger { get; init; }

    #endregion Injections
}