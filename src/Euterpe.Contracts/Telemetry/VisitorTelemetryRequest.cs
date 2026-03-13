namespace Euterpe.Contracts.Telemetry;

[PublicAPI]
public sealed class VisitorTelemetryRequest
{
    public required string Country { get; set; }
    public required string Platform { get; set; }
    public required string Architecture { get; set; }
    public required string AppVersion { get; set; }
}