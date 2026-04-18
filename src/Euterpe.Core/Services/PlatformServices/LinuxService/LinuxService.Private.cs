namespace Euterpe.Core;

internal sealed partial class LinuxService
{
    private async Task ExtractIconAsync(string localAppData)
    {
        var iconDir = Path.Combine(localAppData, "icons", "hicolor", IconHicolorSize, "apps");
        var iconPath = Path.Combine(iconDir, $"{AppId}.png");

        Directory.CreateDirectory(iconDir);

        var stream = ResourceService.GetAssetAsStream(IconAssetName);
        await using (stream.ConfigureAwait(false))
        {
            var destination = new FileStream(iconPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await using (destination.ConfigureAwait(false))
            {
                await stream.CopyToAsync(destination).ConfigureAwait(false);
            }
        }

        Logger.ZLogInformation($"Extracted application icon to {iconPath}");
    }
}