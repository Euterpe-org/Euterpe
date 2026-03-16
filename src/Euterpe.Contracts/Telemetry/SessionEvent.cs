namespace Euterpe.Contracts.Telemetry;

[PublicAPI]
public readonly record struct SessionEvent(string CountryCode, string Platform, string Architecture, string AppVersion);