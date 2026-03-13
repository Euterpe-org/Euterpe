namespace Euterpe.Contracts.Telemetry;

[PublicAPI]
public sealed class ModDownloadEvent
{
    public required string ModName { get; set; }
    public required string ModAuthor { get; set; }
}