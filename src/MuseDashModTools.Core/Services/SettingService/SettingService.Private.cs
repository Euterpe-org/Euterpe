namespace MuseDashModTools.Core;

internal sealed partial class SettingService
{
    private async Task CheckMuseDashFolderAsync()
    {
        if (Config.MuseDashFolder.IsNullOrEmpty())
        {
            Logger.ZLogError($"MuseDash folder is null or empty");
            if (PlatformService.GetGamePath(out var folderPath))
            {
                var result = await MessageBoxService.NoticeConfirmMessageBoxAsync($"Auto detected MuseDash folder\r\n{folderPath}").ConfigureAwait(true);
                Config.MuseDashFolder = result is MessageBoxResult.Yes ? folderPath : await LocalService.GetMuseDashFolderAsync().ConfigureAwait(true);
            }
            else
            {
                Logger.ZLogInformation($"Letting user choose MuseDash folder...");
                Config.MuseDashFolder = await LocalService.GetMuseDashFolderAsync().ConfigureAwait(true);
            }
        }
    }
}