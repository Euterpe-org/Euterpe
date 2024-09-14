namespace MuseDashModToolsUI.Contracts;

public interface ILocalService
{
    Task CheckDotNetRuntimeInstallAsync();
    Task<string> GetMuseDashFolderAsync();
    IEnumerable<string> GetModFiles(string folderPath);
    Task<ModDto?> LoadModFromPathAsync(string filePath);
    void LaunchGame(bool isModded);
}