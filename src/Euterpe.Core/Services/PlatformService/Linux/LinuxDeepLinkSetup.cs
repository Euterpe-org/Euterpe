using CliWrap;
using CliWrap.Buffered;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed class LinuxDeepLinkSetup : IDeepLinkSetup
{
    private const string AppId = "com.euterpe-org.Euterpe";
    private const string DeepLinkDesktopFileName = $"{AppId}.desktop";
    private const string IconAssetName = "Icon.png";
    private const string IconHicolorSize = "256x256";

    public async Task SetupDeepLinkAsync(string processPath)
    {
        try
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var applicationsPath = Path.Combine(localAppData, "applications");
            var desktopFilePath = Path.Combine(applicationsPath, DeepLinkDesktopFileName);
            Directory.CreateDirectory(applicationsPath);

            await ExtractIconAsync(localAppData).ConfigureAwait(false);

            var content =
                $"""
                 [Desktop Entry]
                 Type=Application
                 Name={AppName}
                 Exec="{processPath.EscapeDesktopExecArgument()}" %u
                 Icon={AppId}
                 MimeType=x-scheme-handler/{IDeepLinkSetup.DeepLinkScheme};
                 StartupWMClass={AppName}
                 Terminal=false
                 """;

            await File.WriteAllTextAsync(desktopFilePath, content).ConfigureAwait(false);

            var result = await Cli.Wrap("xdg-mime")
                .WithArguments(["default", DeepLinkDesktopFileName, $"x-scheme-handler/{IDeepLinkSetup.DeepLinkScheme}"])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (result.ExitCode is not 0)
            {
                Logger.ZLogWarning($"xdg-mime exited with code {result.ExitCode}: {result.StandardError}");
                return;
            }

            Logger.ZLogInformation($"Registered deep link protocol on Linux with process path: {processPath}");
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to register deep link protocol on Linux");
        }
    }

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

    #region Injections

    [UsedImplicitly]
    public required ILogger<LinuxDeepLinkSetup> Logger { get; init; }

    [UsedImplicitly]
    public required IResourceService ResourceService { get; init; }

    #endregion Injections
}