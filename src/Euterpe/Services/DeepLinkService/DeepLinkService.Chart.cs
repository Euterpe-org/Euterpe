namespace Euterpe.Services;

public sealed partial class DeepLinkService
{
    private Task HandleChartActionAsync(string path)
    {
        var segments = path.Split('/', 2);

        switch (segments)
        {
            case ["convert"]:
                // Placeholder: legacy chart conversion (CustomAlbums -> epk) is not implemented yet.
                Logger.ZLogInformation($"Chart convert deep link received (not implemented yet)");
                break;

            default:
                Logger.ZLogWarning($"Unknown chart deep link path: {path}");
                break;
        }

        return Task.CompletedTask;
    }
}
