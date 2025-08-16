namespace MuseDashModTools.Updater.Contracts;

public interface ILocalService
{
    bool ExtractZipFile(string zipPath, string extractPath);

    void CopyDirectory(string sourceDir, string destinationDir);
}