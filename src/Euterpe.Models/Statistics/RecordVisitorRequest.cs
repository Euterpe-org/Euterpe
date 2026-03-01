namespace Euterpe.Models.Statistics;

[PublicAPI]
public sealed class RecordVisitorRequest
{
    public string? Country { get; set; }
    public string? Platform { get; set; }
    public string? Arch { get; set; }
    public string? AppVersion { get; set; }
}
