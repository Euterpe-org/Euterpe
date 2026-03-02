namespace Euterpe.Core;

internal sealed partial class UpdateService
{
    private const string GeoDetectionUrl = "https://ipinfo.io/country";

    public async Task ConfigureDownloadSourceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var country = await Client.GetStringAsync(GeoDetectionUrl, cancellationToken).ConfigureAwait(false);
            var downloadSource = country.Trim().Equals("CN", StringComparison.OrdinalIgnoreCase)
                ? DownloadSource.Gitee
                : DownloadSource.GitHub;

            Config.DownloadSource = downloadSource;
            Logger.ZLogInformation($"Download source configured to {downloadSource} based on geo-detection");
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to detect geo location, keeping current download source");
        }
    }
}
