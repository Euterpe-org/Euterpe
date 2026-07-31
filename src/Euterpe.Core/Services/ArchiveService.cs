using System.IO.Compression;

namespace Euterpe.Core;

internal sealed class ArchiveService : IArchiveService
{
    #region Injections

    public required ILogger<ArchiveService> Logger { get; init; }

    #endregion Injections

    public void CreateZipFile(string sourceFolder, string zipPath)
    {
        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }

        ZipFile.CreateFromDirectory(sourceFolder, zipPath, CompressionLevel.Optimal, false);
        Logger.LogInformation($"Successfully created {zipPath} from {sourceFolder}");
    }

    public async Task CreateZipFileAsync(string sourceFolder, string zipPath)
    {
        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }

        await ZipFile.CreateFromDirectoryAsync(sourceFolder, zipPath, CompressionLevel.Optimal, false).ConfigureAwait(false);
        Logger.LogInformation($"Successfully created {zipPath} from {sourceFolder}");
    }

    public void ExtractZipFile(string zipPath, string extractPath)
    {
        ZipFile.ExtractToDirectory(zipPath, extractPath, true);
        Logger.LogInformation($"Successfully extracted {zipPath} to {extractPath}");
    }

    public async Task ExtractZipFileAsync(string zipPath, string extractPath)
    {
        await ZipFile.ExtractToDirectoryAsync(zipPath, extractPath, true).ConfigureAwait(false);
        Logger.LogInformation($"Successfully extracted {zipPath} to {extractPath}");
    }
}
