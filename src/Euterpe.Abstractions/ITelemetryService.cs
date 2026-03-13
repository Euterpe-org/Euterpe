namespace Euterpe.Abstractions;

public interface ITelemetryService
{
    Task TrackVisitorAsync();
    Task TrackModDownloadAsync(string modName, string modAuthor);
}