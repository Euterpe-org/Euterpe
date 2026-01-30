namespace Euterpe.Core;

internal sealed partial class SettingService
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

    private async Task CheckMuseDashFolderAsync()
    {
        if (Config.MuseDashFolder.IsNullOrEmpty() || !PlatformService.CheckIsValidGameFolder(Config.MuseDashFolder))
        {
            Logger.ZLogError($"Stored MuseDash folder is invalid");

            var useDetectedPath = false;
            if (PlatformService.TryGetGameFolder(out var gameFolder))
            {
                var result = await MessageBoxService.NoticeConfirmOverlayAsync(MessageBox_Content_Confirm_DetectedMuseDashPath, gameFolder).ConfigureAwait(true);
                useDetectedPath = result is MessageBoxResult.Yes;
            }

            if (useDetectedPath)
            {
                Config.MuseDashFolder = gameFolder;
            }
            else
            {
                Logger.ZLogInformation($"Letting user choose MuseDash folder...");
                var result = await MessageBoxService.NoticeConfirmOverlayAsync(MessageBox_Content_ChooseMuseDashFolder).ConfigureAwait(true);
                if (result is not MessageBoxResult.Yes)
                {
                    Logger.ZLogInformation($"User cancelled MuseDash folder selection. Exiting application.");
                    Environment.Exit(0);
                }

                Config.MuseDashFolder = await LocalService.GetMuseDashFolderAsync().ConfigureAwait(true);
            }
        }
    }

    private void CreateNecessaryFolders()
    {
        Directory.CreateDirectory(Config.ModsFolder);
        Directory.CreateDirectory(Config.UserLibsFolder);
        Directory.CreateDirectory(Config.CustomAlbumsFolder);
    }
}