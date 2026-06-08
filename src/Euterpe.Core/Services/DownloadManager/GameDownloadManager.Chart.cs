using static Euterpe.Models.Charts.ChartFiles;

namespace Euterpe.Core;

internal sealed partial class GameDownloadManager
{
    private async Task PopulateChartWorkFolderAsync(string cid, string workFolder, CancellationToken cancellationToken)
    {
        FileSystemService.TryDeleteDirectory(workFolder, DeleteOption.IgnoreIfNotFound);
        Directory.CreateDirectory(workFolder);

        await DownloadChartFileAsync(cid, workFolder, ManifestFileName, cancellationToken).ConfigureAwait(false);

        var manifestPath = Path.Combine(workFolder, ManifestFileName);
        var manifest = await MessagePackSerialization.DeserializeManifestFromFileAsync(manifestPath, cancellationToken).ConfigureAwait(false);

        foreach (var fileName in manifest.Files.Keys.Where(fileName => fileName != ManifestFileName))
        {
            await DownloadChartFileAsync(cid, workFolder, fileName, cancellationToken).ConfigureAwait(false);
        }
    }

    private Task DownloadChartFileAsync(string cid, string workFolder, string fileName, CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(workFolder, fileName);
        return AppDownloadManager.DownloadAssetAsync(ChartFileUrl(cid, fileName), filePath, $"chart {cid}/{fileName}", cancellationToken);
    }

    private static string ChartFileUrl(string cid, string fileName) =>
        $"{EuterpeDownload.BaseUrl}{EuterpeDownload.Charts.BasePath}/{cid}/{fileName}";
}