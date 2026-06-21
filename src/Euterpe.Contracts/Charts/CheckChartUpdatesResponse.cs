namespace Euterpe.Contracts.Charts;

[PublicAPI]
public sealed class CheckChartUpdatesResponse
{
    public Dictionary<string, ChartUpdateDelta> Charts { get; set; } = [];
}
