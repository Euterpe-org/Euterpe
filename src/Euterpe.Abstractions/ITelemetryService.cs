namespace Euterpe.Abstractions;

public interface ITelemetryService
{
    Task TrackSessionAsync();
    Task TrackModDownloadAsync(string modName, string modAuthor);
}