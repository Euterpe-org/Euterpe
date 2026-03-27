using CliWrap;
using CliWrap.Buffered;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed partial class LinuxService : IPlatformService
{
    private const string DeepLinkDesktopFileName = "com.euterpe-org.Euterpe.desktop";

    public string OsString => "linux";
    public string UpdaterFileName => "Updater";

    public async Task SetupDeepLinkAsync(string processPath)
    {
        try
        {
            var applicationsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "applications");
            var desktopFilePath = Path.Combine(applicationsPath, DeepLinkDesktopFileName);
            Directory.CreateDirectory(applicationsPath);

            var content =
                $"""
                 [Desktop Entry]
                 Type=Application
                 Name=Euterpe
                 Exec="{processPath.EscapeDesktopExecArgument()}" %u
                 MimeType=x-scheme-handler/{IPlatformService.DeepLinkScheme};
                 Terminal=false
                 """;

            await File.WriteAllTextAsync(desktopFilePath, content).ConfigureAwait(false);

            var result = await Cli.Wrap("xdg-mime")
                .WithArguments(["default", DeepLinkDesktopFileName, $"x-scheme-handler/{IPlatformService.DeepLinkScheme}"])
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

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required TopLevelProxy TopLevel { get; init; }

    [UsedImplicitly]
    public required IDownloadManager DownloadManager { get; init; }

    [UsedImplicitly]
    public required IFileSystemService FileSystemService { get; init; }

    [UsedImplicitly]
    public required ILogger<LinuxService> Logger { get; init; }

    [UsedImplicitly]
    public required IGamePathService GamePathService { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}