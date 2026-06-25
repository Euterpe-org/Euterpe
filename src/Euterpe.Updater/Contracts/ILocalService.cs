namespace Euterpe.Updater.Contracts;

public interface ILocalService
{
    void ExtractZipFile(string zipPath, string extractPath);
    void CopyDirectory(string sourceDir, string destinationDir);
}
