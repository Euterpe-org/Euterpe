namespace Euterpe.Contracts.Charts;

[PublicAPI]
public sealed class ChartUpdateDelta
{
    public string[] Changed { get; set; } = [];
    public string[] Deleted { get; set; } = [];
}
