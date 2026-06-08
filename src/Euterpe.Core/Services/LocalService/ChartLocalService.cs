using static Euterpe.Models.Charts.ChartFiles;
using static Euterpe.Models.Charts.CustomAlbums.CustomAlbumFiles;

namespace Euterpe.Core;

internal sealed class ChartLocalService : IChartLocalService
{
    public IEnumerable<string> GetChartFolderPaths(ChartSource source) =>
        Directory.EnumerateDirectories(
            source switch
            {
                ChartSource.Online => GameConfig.OnlineChartsFolder,
                ChartSource.Offline => GameConfig.OfflineChartsFolder,
                _ => throw new UnreachableException()
            });

    public string[] GetCustomAlbumsChartFilePaths() => Directory.EnumerateFiles(GameConfig.CustomAlbumsChartsFolder)
        .Where(x => Path.GetExtension(x) is PackageExtension)
        .ToArray();

    public async Task<ChartDto?> LoadChartFromPathAsync(string chartFolder, ChartSource source)
    {
        var manifestPath = Path.Combine(chartFolder, ManifestFileName);
        if (!File.Exists(manifestPath))
        {
            return null;
        }

        try
        {
            var stream = File.OpenRead(manifestPath);
            await using (stream.ConfigureAwait(false))
            {
                var manifest = await MessagePackSerialization.DeserializeManifestAsync(stream).ConfigureAwait(false);

                return new ChartDto
                {
                    FolderPath = chartFolder,
                    FolderName = Path.GetFileName(chartFolder),
                    Manifest = manifest,
                    Source = source
                };
            }
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to load chart from {chartFolder}, skipping");
            return null;
        }
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required IMessagePackSerializationService MessagePackSerialization { get; init; }
    public required ILogger<ChartLocalService> Logger { get; init; }

    #endregion Injections
}