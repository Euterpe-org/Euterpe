namespace Euterpe.Contracts.Telemetry;

[PublicAPI]
public sealed class DownloadTelemetryRequest
{
    public required string ModName { get; set; }
    public required string ModAuthor { get; set; }
}