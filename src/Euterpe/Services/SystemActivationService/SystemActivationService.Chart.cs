namespace Euterpe.Services;

public sealed partial class SystemActivationService
{
    private async Task HandleChartActionAsync(string path)
    {
        var segments = path.Split('/', 2);

        switch (segments)
        {
            case ["convert"]:
                Logger.ZLogInformation($"Chart convert deep link received, migrating CustomAlbums charts");
                await ChartManageService.MigrateCustomAlbumsAsync().ConfigureAwait(false);
                break;

            case ["download", var chartId]:
                await ChartManageService.DownloadChartAsync(chartId).ConfigureAwait(false);
                break;

            case ["update"]:
                await ChartManageService.UpdateAllChartsAsync().ConfigureAwait(false);
                break;

            case ["update", var chartId]:
                await ChartManageService.UpdateChartAsync(chartId).ConfigureAwait(false);
                break;

            case ["remove", var folderPath]:
                await ChartManageService.RemoveChartAsync(folderPath).ConfigureAwait(false);
                break;

            default:
                Logger.ZLogWarning($"Unknown chart deep link path: {path}");
                break;
        }
    }
}
