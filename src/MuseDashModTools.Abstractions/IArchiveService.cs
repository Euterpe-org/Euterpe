namespace MuseDashModTools.Abstractions;

public interface IArchiveService
{
    bool CreateZipFile(string sourceFolder, string zipPath);
    Task<bool> CreateZipFileAsync(string sourceFolder, string zipPath);
    bool ExtractZipFile(string zipPath, string extractPath);
    Task<bool> ExtractZipFileAsync(string zipPath, string extractPath);
}