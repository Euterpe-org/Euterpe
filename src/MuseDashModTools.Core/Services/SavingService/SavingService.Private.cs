namespace MuseDashModTools.Core;

internal sealed partial class SavingService
{
    private async Task CheckValidSettingAsync()
    {
        Logger.ZLogInformation($"Checking for valid setting...");
        await CheckMuseDashFolderAsync().ConfigureAwait(true);

        Logger.ZLogInformation($"Checking for valid setting done");
    }

    private async Task CheckMuseDashFolderAsync()
    {
        if (Setting.MuseDashFolder.IsNullOrEmpty())
        {
            Logger.ZLogError($"MuseDash folder is null or empty");
            if (PlatformService.GetGamePath(out var folderPath))
            {
                var result = await MessageBoxService.NoticeConfirmMessageBoxAsync($"Auto detected MuseDash folder\r\n{folderPath}").ConfigureAwait(true);
                Setting.MuseDashFolder = result is MessageBoxResult.Yes ? folderPath : await LocalService.GetMuseDashFolderAsync().ConfigureAwait(true);
            }
            else
            {
                Logger.ZLogInformation($"Letting user choose MuseDash folder...");
                Setting.MuseDashFolder = await LocalService.GetMuseDashFolderAsync().ConfigureAwait(true);
            }
        }
    }
}