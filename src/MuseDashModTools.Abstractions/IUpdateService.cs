namespace MuseDashModTools.Abstractions;

public interface IUpdateService
{
    Task CheckForUpdatesAsync(CancellationToken cancellationToken = default);
}