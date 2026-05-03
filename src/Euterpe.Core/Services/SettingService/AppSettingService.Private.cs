namespace Euterpe.Core;

internal sealed partial class AppSettingService
{
    private async Task CheckSteamFolderAsync()
    {
        if (Config.SteamFolder.IsNullOrEmpty() || !PlatformService.CheckIsValidSteamFolder(Config.SteamFolder))
        {
            Logger.ZLogError($"Stored Steam folder is invalid");

            if (PlatformService.TryGetSteamFolder(out var steamFolder))
            {
                Config.SteamFolder = steamFolder;
            }
            else
            {
                Logger.ZLogInformation($"Letting user choose Steam folder...");
                await MessageBoxService.NoticeOverlayAsync(MessageBox_Content_ChooseSteamFolder).ConfigureAwait(true);
                Config.SteamFolder = await LocalService.GetSteamFolderAsync().ConfigureAwait(true);
            }
        }
    }

    private async Task CheckSteamExecPathAsync()
    {
        if (Config.SteamExecPath.IsNullOrEmpty() || !PlatformService.CheckIsValidSteamExecPath(Config.SteamExecPath))
        {
            Logger.ZLogError($"Stored Steam executable is invalid");

            var detectedPath = await PlatformService.GetSteamExecPathAsync().ConfigureAwait(true);
            if (!detectedPath.IsNullOrEmpty())
            {
                Config.SteamExecPath = detectedPath;
            }
            else
            {
                Logger.ZLogInformation($"Letting user choose Steam executable...");
                await MessageBoxService.NoticeOverlayAsync(MessageBox_Content_ChooseSteamExec).ConfigureAwait(true);
                Config.SteamExecPath = await LocalService.GetSteamExecPathAsync().ConfigureAwait(true);
            }
        }
    }
}