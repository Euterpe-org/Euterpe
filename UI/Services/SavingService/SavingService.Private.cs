namespace MuseDashModToolsUI.Services;

public sealed partial class SavingService
{
    private async Task CheckValidSettingAsync()
    {
        if (Setting.MuseDashFolder.IsNullOrEmpty())
        {
            Setting.MuseDashFolder = await LocalService.GetMuseDashFolderAsync().ConfigureAwait(false);
        }
    }
}