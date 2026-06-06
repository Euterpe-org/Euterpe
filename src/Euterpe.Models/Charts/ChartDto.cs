namespace Euterpe.Models.Charts;

public sealed class ChartDto : ObservableObject
{
    public required string FolderPath { get; init; }
    public required Manifest Manifest { get; init; }
    public ChartSource Source { get; set; } = ChartSource.Offline;
}