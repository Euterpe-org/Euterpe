using Euterpe.Models.Charts.CustomAlbums;

namespace Euterpe.Abstractions;

public interface IMigrationService
{
    Task<MigrationOutcome> MigrateCustomAlbumAsync(CustomAlbumSource source, CancellationToken cancellationToken = default);
    Task MigrateCustomAlbumsAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default);
}
