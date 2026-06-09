using Euterpe.Models.Charts.CustomAlbums;
using static Euterpe.Models.Charts.ChartFiles;
using static Euterpe.Models.Charts.CustomAlbums.CustomAlbumFiles;

namespace Euterpe.Core;

internal sealed partial class ChartLocalService : IChartLocalService
{
    public IEnumerable<string> GetChartFolderPaths(ChartSource source) =>
        Directory.EnumerateDirectories(
            source switch
            {
                ChartSource.Online => GameConfig.OnlineChartsFolder,
                ChartSource.Offline => GameConfig.OfflineChartsFolder,
                _ => throw new UnreachableException()
            });

    public CustomAlbumSource[] GetCustomAlbumsSources()
    {
        var root = GameConfig.CustomAlbumsChartsFolder;
        var packages = Directory.EnumerateFiles(root).Where(x => Path.GetExtension(x) is PackageExtension);
        var folders = Directory.EnumerateDirectories(root).Where(d => File.Exists(Path.Combine(d, InfoFileName)));

        return ResolveSources([.. packages, .. folders]);
    }

    public async Task<ChartDto?> LoadChartFromPathAsync(string chartFolder, ChartSource source)
    {
        var manifestPath = Path.Combine(chartFolder, ManifestFileName);
        if (!File.Exists(manifestPath))
        {
            return null;
        }

        try
        {
            var manifest = await MessagePackSerialization.DeserializeManifestFromFileAsync(manifestPath).ConfigureAwait(false);

            return new ChartDto
            {
                FolderPath = chartFolder,
                FolderName = Path.GetFileName(chartFolder),
                Manifest = manifest,
                Source = source
            };
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
