using System.IO.Compression;

namespace MuseDashModTools.Updater.Services;

public sealed class LocalService(ILogger<LocalService> logger) : ILocalService
{
    private readonly ILogger<LocalService> _logger = logger;

    public bool ExtractZipFile(string zipPath, string extractPath)
    {
        try
        {
            ZipFile.ExtractToDirectory(zipPath, extractPath, true);
            _logger.ZLogInformation($"Successfully extracted {zipPath} to {extractPath}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex, $"Failed to extract file {zipPath} to {extractPath}");
            return false;
        }
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
}