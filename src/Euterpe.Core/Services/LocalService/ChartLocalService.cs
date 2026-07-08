using Euterpe.Models.Charts.CustomAlbums;
using static Euterpe.Models.Charts.ChartFiles;

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

    public CustomAlbumSource CreateCustomAlbumSource(string path) => new(path, Directory.Exists(path));

    public CustomAlbumSource[] GetCustomAlbumSources()
    {
        var root = GameConfig.CustomAlbumsChartsFolder;
        var packages = Directory.EnumerateFiles(root).Where(x => Path.GetExtension(x) is CustomAlbumFiles.PackageExtension);
        var folders = Directory.EnumerateDirectories(root).Where(d => File.Exists(Path.Combine(d, CustomAlbumFiles.InfoFileName)));

        return
        [
            .. packages.Select(static path => new CustomAlbumSource(path, false)),
            .. folders.Select(static path => new CustomAlbumSource(path, true))
        ];
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
