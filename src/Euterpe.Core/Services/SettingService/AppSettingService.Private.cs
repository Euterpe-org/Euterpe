namespace Euterpe.Core;

internal sealed partial class AppSettingService
{
    private void BackupCorruptedConfig(Exception cause)
    {
        var backupPath = $"{ConfigPath}.corrupted-{DateTime.Now:yyyyMMdd-HHmmss}";
        try
        {
            File.Move(ConfigPath, backupPath);
            Logger.ZLogError(cause, $"Config file is corrupted; backed up to {backupPath} and using default settings");
        }
        catch (Exception moveEx)
        {
            Logger.ZLogError(moveEx, $"Config file is corrupted but could not back up; using default settings. Original cause: {cause.Message}");
        }
    }

    private async Task CheckSteamFolderAsync()
    {
        if (Config.SteamFolder.IsNullOrEmpty() || !SteamDiscovery.CheckIsValidSteamFolder(Config.SteamFolder))
        {
            Logger.ZLogError($"Stored Steam folder is invalid");

            if (SteamDiscovery.TryGetSteamFolder(out var steamFolder))
            {
                Config.SteamFolder = steamFolder;
            }
            else
            {
                Logger.ZLogInformation($"Letting user choose Steam folder...");
                await MessageBoxService.NoticeOverlayAsync(MessageBox_Content_ChooseSteamFolder).ConfigureAwait(true);
                Config.SteamFolder = await AppLocalService.GetSteamFolderAsync().ConfigureAwait(true);
            }
        }
    }

    private async Task CheckSteamExecPathAsync()
    {
        if (Config.SteamExecPath.IsNullOrEmpty() || !SteamDiscovery.CheckIsValidSteamExecPath(Config.SteamExecPath))
        {
            Logger.ZLogError($"Stored Steam executable is invalid");

            var detectedPath = await SteamDiscovery.GetSteamExecPathAsync().ConfigureAwait(true);
            if (!detectedPath.IsNullOrEmpty())
            {
                Config.SteamExecPath = detectedPath;
            }
            else
            {
                Logger.ZLogInformation($"Letting user choose Steam executable...");
                await MessageBoxService.NoticeOverlayAsync(MessageBox_Content_ChooseSteamExec).ConfigureAwait(true);
                Config.SteamExecPath = await AppLocalService.GetSteamExecPathAsync().ConfigureAwait(true);
            }
        }
    }
}