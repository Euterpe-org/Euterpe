namespace MuseDashModTools.Abstractions;

public interface IArchiveService
{
    bool CreateZipFile(string sourceFolder, string zipPath);
    bool ExtractZipFile(string zipPath, string extractPath);
}