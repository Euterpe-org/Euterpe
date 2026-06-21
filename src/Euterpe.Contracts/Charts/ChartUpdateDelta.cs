namespace Euterpe.Contracts.Charts;

[PublicAPI]
public sealed class ChartUpdateDelta
{
    public List<string> Changed { get; set; } = [];

    public List<string> Deleted { get; set; } = [];
}
