namespace Euterpe.Contracts.Charts;

[PublicAPI]
public sealed class CheckChartUpdatesResponse
{
    public Dictionary<string, Dictionary<string, ChartFileEntry>> Updates { get; set; } = [];

    public int[] Removed { get; set; } = [];
}