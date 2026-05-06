namespace Euterpe.Abstractions;

public interface IArchiveService
{
    void CreateZipFile(string sourceFolder, string zipPath);
    Task CreateZipFileAsync(string sourceFolder, string zipPath);
    void ExtractZipFile(string zipPath, string extractPath);
    Task ExtractZipFileAsync(string zipPath, string extractPath);
}