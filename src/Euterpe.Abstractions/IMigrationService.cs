using Euterpe.Models.Charts.CustomAlbums;

namespace Euterpe.Abstractions;

public interface IMigrationService
{
    Task<(MigrationOutcome Outcome, string Destination)> MigrateCustomAlbumAsync(CustomAlbumSource source, CancellationToken cancellationToken = default);
}
