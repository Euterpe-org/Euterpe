using System.IO.Compression;

namespace Euterpe.Updater.Services;

public sealed class LocalService(ILogger<LocalService> logger) : ILocalService
{
    private readonly ILogger<LocalService> _logger = logger;

    public bool IsReadableZipFile(string zipPath)
    {
        try
        {
            using var archive = ZipFile.OpenRead(zipPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex, $"Cannot open zip file at {zipPath}");
            return false;
        }
    }

    public void ExtractZipFile(string zipPath, string extractPath)
    {
        ZipFile.ExtractToDirectory(zipPath, extractPath, true);
        _logger.ZLogInformation($"Successfully extracted {zipPath} to {extractPath}");
    }

    public void CopyDirectory(string sourceDir, string destinationDir)
    {
        foreach (var filePath in Directory.EnumerateFiles(sourceDir))
        {
            var fileName = Path.GetFileName(filePath);
            var destinationPath = Path.Combine(destinationDir, fileName);
            File.Copy(filePath, destinationPath, true);
        }

        _logger.ZLogInformation($"Directory copied from {sourceDir} to {destinationDir}");
    }

    public bool TryDeleteFile(string filePath)
    {
        try
        {
            File.Delete(filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.ZLogWarning(ex, $"Failed to delete {filePath}");
            return false;
        }
    }
}
