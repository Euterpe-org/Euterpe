namespace Euterpe.Abstractions;

public interface ITelemetryService
{
    Task TrackVisitorAsync();
    Task TrackDownloadAsync(string modName, string modAuthor);
}