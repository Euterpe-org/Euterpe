namespace MuseDashModTools.Core;

internal sealed partial class LocalService
{
    private async Task<bool> CheckValidPathAsync(string folderPath)
    {
        var exePath = Path.Combine(folderPath, "MuseDash.exe");
        var dllPath = Path.Combine(folderPath, "GameAssembly.dll");

        if (!File.Exists(exePath) || !File.Exists(dllPath))
        {
            Logger.ZLogError($"MuseDash.exe or GameAssembly.dll not found in {folderPath}");
            await MessageBoxService.ErrorMessageBoxAsync(MsgBox_Content_InvalidPath).ConfigureAwait(true);
            return false;
        }

        Logger.ZLogInformation($"MuseDash.exe and GameAssembly.dll found in {folderPath}");
        return true;
    }
}