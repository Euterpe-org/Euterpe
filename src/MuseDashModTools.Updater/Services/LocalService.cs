using System.IO.Compression;
using Microsoft.Extensions.Logging;

namespace MuseDashModTools.Updater.Services;

public sealed class LocalService(ILogger<LocalService> logger) : ILocalService
{
    private readonly ILogger<LocalService> _logger = logger;

    public bool ExtractZipFile(string zipPath, string extractPath)
    {
        try
        {
            ZipFile.ExtractToDirectory(zipPath, extractPath, true);
            _logger.LogInformation("Successfully extracted {ZipPath} to {ExtractPath}", zipPath, extractPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract file {ZipPath} to {ExtractPath}", zipPath, extractPath);
            return false;
        }
    }
}