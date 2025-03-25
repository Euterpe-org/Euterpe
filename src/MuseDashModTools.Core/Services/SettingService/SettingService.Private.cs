namespace MuseDashModTools.Core;

internal sealed partial class SettingService
{
    private async Task CheckMuseDashFolderAsync()
    {
        if (Config.MuseDashFolder.IsNullOrEmpty())
        {
            Logger.ZLogError($"MuseDash folder is null or empty");

            var useDetectedPath = false;
            if (PlatformService.GetGamePath(out var folderPath))
            {
                var result = await MessageBoxService.NoticeConfirmOverlayAsync(MessageBox_Content_Confirm_DetectedMuseDashPath, folderPath)
                    .ConfigureAwait(true);
                useDetectedPath = result is MessageBoxResult.Yes;
            }

            if (useDetectedPath)
            {
                Config.MuseDashFolder = folderPath;
            }
            else
            {
                Logger.ZLogInformation($"Letting user choose MuseDash folder...");
                await MessageBoxService.NoticeOverlayAsync(MessageBox_Content_Notice_ChooseMuseDashPath).ConfigureAwait(true);
                Config.MuseDashFolder = await LocalService.GetMuseDashFolderAsync().ConfigureAwait(true);
            }
        }
    }
}