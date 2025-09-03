using System.IO.Compression;

namespace MuseDashModTools.Core;

internal sealed class ArchiveService : IArchiveService
{
    #region Injections

    [UsedImplicitly]
    public required ILogger<ArchiveService> Logger { get; init; }

    #endregion Injections

    public bool CreateZipFile(string sourceFolder, string zipPath)
    {
        try
        {
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            ZipFile.CreateFromDirectory(sourceFolder, zipPath, CompressionLevel.Optimal, false);
            Logger.ZLogInformation($"Successfully created {zipPath} from {sourceFolder}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to create zip file {zipPath} from {sourceFolder}");
            return false;
        }
    }

    public bool ExtractZipFile(string zipPath, string extractPath)
    {
        try
        {
            ZipFile.ExtractToDirectory(zipPath, extractPath, true);
            Logger.ZLogInformation($"Successfully extracted {zipPath} to {extractPath}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to extract file {zipPath} to {extractPath}");
            return false;
        }
    }
}