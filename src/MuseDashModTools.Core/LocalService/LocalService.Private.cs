namespace MuseDashModTools.Core;

public sealed partial class LocalService
{
    private async Task<bool> CheckValidPathAsync(string folderPath)
    {
        var exePath = Path.Combine(folderPath, "MuseDash.exe");
        var dllPath = Path.Combine(folderPath, "GameAssembly.dll");

        if (!File.Exists(exePath) || !File.Exists(dllPath))
        {
            Logger.Error("MuseDash.exe or GameAssembly.dll not found in {FolderPath}", folderPath);
            await MessageBoxService.ErrorMessageBoxAsync(MsgBox_Content_InvalidPath).ConfigureAwait(true);
            return false;
        }

        Logger.Information("MuseDash.exe and GameAssembly.dll found in {FolderPath}", folderPath);
        return true;
    }
}