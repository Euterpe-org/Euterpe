namespace Euterpe.Contracts.Telemetry;

[PublicAPI]
public readonly record struct VisitorEvent(string CountryCode, string Platform, string Architecture, string AppVersion);