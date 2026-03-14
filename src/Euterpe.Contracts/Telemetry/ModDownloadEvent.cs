namespace Euterpe.Contracts.Telemetry;

[PublicAPI]
public readonly record struct ModDownloadEvent(string ModName, string ModAuthor);