namespace Euterpe.Contracts.Telemetry;

[PublicAPI]
public record SessionEvent(string Country, string Platform, string Arch, string AppVersion);