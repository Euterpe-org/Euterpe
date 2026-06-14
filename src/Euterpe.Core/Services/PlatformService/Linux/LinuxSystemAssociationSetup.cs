using CliWrap;
using CliWrap.Buffered;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed class LinuxSystemAssociationSetup : ISystemAssociationSetup
{
    private const string AppId = "com.euterpe-org.Euterpe";
    private const string DeepLinkDesktopFileName = $"{AppId}.desktop";
    private const string IconAssetName = "Icon.png";
    private const string IconHicolorSize = "256x256";
    private const string EpkMimeType = "application/x-euterpe-epk";
    private const string MimePackageFileName = $"{AppId}.xml";

    public async Task RegisterAsync(string processPath)
    {
        try
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var applicationsPath = Path.Combine(localAppData, "applications");
            var desktopFilePath = Path.Combine(applicationsPath, DeepLinkDesktopFileName);
            Directory.CreateDirectory(applicationsPath);

            await ExtractIconAsync(localAppData).ConfigureAwait(false);
            await InstallEpkMimeTypeAsync(localAppData).ConfigureAwait(false);

            var content =
                $"""
                 [Desktop Entry]
                 Type=Application
                 Name={AppName}
                 Exec="{processPath.EscapeDesktopExecArgument()}" %u
                 Icon={AppId}
                 MimeType=x-scheme-handler/{ISystemAssociationSetup.DeepLinkScheme};{EpkMimeType};
                 StartupWMClass={AppName}
                 Terminal=false
                 """;

            await File.WriteAllTextAsync(desktopFilePath, content).ConfigureAwait(false);

            var result = await Cli.Wrap("xdg-mime")
                .WithArguments(["default", DeepLinkDesktopFileName, $"x-scheme-handler/{ISystemAssociationSetup.DeepLinkScheme}", EpkMimeType])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (result.ExitCode is not 0)
            {
                Logger.ZLogWarning($"xdg-mime exited with code {result.ExitCode}: {result.StandardError}");
                return;
            }

            Logger.ZLogInformation($"Registered deep link protocol and file association on Linux with process path: {processPath}");
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

    private async Task InstallEpkMimeTypeAsync(string localAppData)
    {
        var mimeDir = Path.Combine(localAppData, "mime");
        var mimePackagesDir = Path.Combine(mimeDir, "packages");
        var mimePackagePath = Path.Combine(mimePackagesDir, MimePackageFileName);

        Directory.CreateDirectory(mimePackagesDir);

        var package =
            $"""
             <?xml version="1.0" encoding="UTF-8"?>
             <mime-info xmlns="http://www.freedesktop.org/standards/shared-mime-info">
                 <mime-type type="{EpkMimeType}">
                     <comment>{AppName} Chart</comment>
                     <glob pattern="*{ChartFiles.ManifestExtension}"/>
                 </mime-type>
             </mime-info>
             """;

        await File.WriteAllTextAsync(mimePackagePath, package).ConfigureAwait(false);

        var result = await Cli.Wrap("update-mime-database")
            .WithArguments([mimeDir])
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync()
            .ConfigureAwait(false);

        if (result.ExitCode is not 0)
        {
            Logger.ZLogWarning($"update-mime-database exited with code {result.ExitCode}: {result.StandardError}");
            return;
        }

        Logger.ZLogInformation($"Installed EPK MIME type to {mimePackagePath}");
    }

    #region Injections

    public required ILogger<LinuxSystemAssociationSetup> Logger { get; init; }
    public required IResourceService ResourceService { get; init; }

    #endregion Injections
}
