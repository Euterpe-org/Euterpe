namespace Euterpe.Services;

public sealed partial class DeepLinkService
{
    private async Task HandleChartActionAsync(string path)
    {
        var segments = path.Split('/', 2);

        switch (segments)
        {
            case ["convert"]:
                Logger.ZLogInformation($"Chart convert deep link received, migrating CustomAlbums charts");
                await MigrationService.MigrateCustomAlbumsAsync().ConfigureAwait(false);
                break;

            default:
                Logger.ZLogWarning($"Unknown chart deep link path: {path}");
                break;
        }
    }
}