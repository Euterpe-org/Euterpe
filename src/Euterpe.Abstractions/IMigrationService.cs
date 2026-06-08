namespace Euterpe.Abstractions;

public interface IMigrationService
{
    Task MigrateCustomAlbumsAsync(
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);
}