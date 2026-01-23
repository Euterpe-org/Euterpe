namespace Euterpe.Models.Statistics;

[PublicAPI]
public sealed class RecordDownloadRequest
{
    public required string Name { get; set; }
    public required string Author { get; set; }
}
