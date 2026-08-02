namespace Euterpe.Services;

public sealed partial class SystemActivationService
{
    private async Task HandleChartActionAsync(string path)
    {
        var segments = path.Split('/', 2);

        switch (segments)
        {
            case ["convert"]:
                await ChartManagePanelViewModel.MigrateCustomAlbumsAsync().ConfigureAwait(false);
                break;

            case ["download", var cid]:
                await ChartManagePanelViewModel.DownloadChartAsync(cid).ConfigureAwait(false);
                break;

            case ["update"]:
                await ChartManageService.UpdateAllChartsAsync().ConfigureAwait(false);
                break;

            case ["update", var cid]:
                await ChartManageService.UpdateChartAsync(cid).ConfigureAwait(false);
                break;

            default:
                Logger.LogWarning("Unknown chart deep link path: {Path}", path);
                break;
        }
    }
}
