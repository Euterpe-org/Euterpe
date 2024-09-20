namespace MuseDashModTools.Contracts;

public interface ILocalService
{
    Task CheckDotNetRuntimeInstallAsync();
    Task<string> GetMuseDashFolderAsync();
    IEnumerable<string> GetModFilePaths();
    HashSet<string?> GetLibFileNames();
    Task<ModDto?> LoadModFromPathAsync(string filePath);
    void LaunchGame(bool isModded);
    bool UnzipFile(string zipPath, string extractPath);
}