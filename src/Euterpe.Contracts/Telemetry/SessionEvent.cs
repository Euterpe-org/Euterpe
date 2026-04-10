namespace Euterpe.Contracts.Telemetry;

[PublicAPI]
public sealed record SessionEvent(string Country, string Platform, string Arch, string AppVersion);