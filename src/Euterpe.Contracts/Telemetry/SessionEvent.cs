namespace Euterpe.Contracts.Telemetry;

[PublicAPI]
public readonly record struct SessionEvent(string Country, string Platform, string Arch, string AppVersion);