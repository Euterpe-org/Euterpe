namespace Euterpe.Abstractions;

public interface ITelemetryService
{
    Task TrackSessionAsync();
}
