namespace Euterpe.Updater.Contracts;

public interface ILocalService
{
    bool IsReadableZipFile(string zipPath);
    void ExtractZipFile(string zipPath, string extractPath);
    void CopyDirectory(string sourceDir, string destinationDir);
    bool TryDeleteFile(string filePath);
    bool TryDeleteDirectory(string directoryPath);
}
