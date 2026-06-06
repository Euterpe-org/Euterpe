namespace Euterpe.Models.Charts;

public sealed class ChartDto : ObservableObject
{
    public Manifest Manifest { get; init; }
    public ChartSource Source { get; set; } = ChartSource.Offline;
}