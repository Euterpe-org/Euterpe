namespace Euterpe.Models.Statistics;

[PublicAPI]
public sealed class VisitorTelemetryRequest
{
    public string? Country { get; set; }
    public string? Platform { get; set; }
    public string? Architecture { get; set; }
    public string? AppVersion { get; set; }
}