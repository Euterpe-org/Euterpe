namespace Euterpe.Contracts.Charts;

[PublicAPI]
public sealed class CheckChartUpdatesRequest
{
    public Dictionary<string, Dictionary<string, ChartFileEntry>> Charts { get; set; } = [];
}
