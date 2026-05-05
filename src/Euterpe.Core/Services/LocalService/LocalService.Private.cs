namespace Euterpe.Core;

internal sealed partial class LocalService
{
    private async ValueTask<bool> EnsureValidSteamFolderAsync(string folderPath)
    {
        if (SteamDiscovery.CheckIsValidSteamFolder(folderPath))
        {
            return true;
        }

        await MessageBoxService.ErrorAsync(MessageBox_Content_InvalidPath).ConfigureAwait(true);
        return false;
    }

    private async ValueTask<bool> EnsureValidSteamExecPathAsync(string execPath)
    {
        if (SteamDiscovery.CheckIsValidSteamExecPath(execPath))
        {
            return true;
        }

        await MessageBoxService.ErrorAsync(MessageBox_Content_InvalidPath).ConfigureAwait(true);
        return false;
    }

    private async ValueTask<bool> EnsureValidGameFolderAsync(string folderPath)
    {
        if (GamePaths.CheckIsValidGameFolder(folderPath))
        {
            return true;
        }

        await MessageBoxService.ErrorAsync(MessageBox_Content_InvalidPath).ConfigureAwait(true);
        return false;
    }

    private static string? ReadFileVersion(string filePath)
    {
        var versionInfo = FileVersionInfo.GetVersionInfo(filePath);
        return versionInfo.FileVersion;
    }
}