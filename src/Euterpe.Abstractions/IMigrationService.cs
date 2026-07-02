using Euterpe.Models.Charts.CustomAlbums;

namespace Euterpe.Abstractions;

public interface IMigrationService
{
    Task<MigrationResult> MigrateCustomAlbumAsync(CustomAlbumSource source, CancellationToken cancellationToken = default);
}
