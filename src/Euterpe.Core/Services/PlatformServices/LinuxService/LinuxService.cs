using System.Text;
using System.Text.Json;
using CliWrap;
using CliWrap.Buffered;
using Euterpe.Contracts.Account;
using static Euterpe.Core.JsonContexts.SnakeCaseJsonContext;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed partial class LinuxService : IPlatformService
{
    private const string AppId = "com.euterpe-org.Euterpe";
    private const string DeepLinkDesktopFileName = $"{AppId}.desktop";
    private const string IconAssetName = "Icon.png";
    private const string IconHicolorSize = "256x256";
    private const string MuseDashRegistryPath = @"HKCU\Software\PeroPeroGames\MuseDash";
    private const string UserInfoValueName = "peropero_account_user_info_h3003705636";

    private const string MuseDashUserInfoCommand = $"""
                                                    wine reg query "{MuseDashRegistryPath}" /v "{UserInfoValueName}"
                                                    """;

    public string OsString => "linux";
    public string UpdaterFileName => "Updater";

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
                 MimeType=x-scheme-handler/{IPlatformService.DeepLinkScheme};
                 StartupWMClass={AppName}
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

    public async Task<MuseDashUidRequest?> GetMuseDashUidRequestAsync()
    {
        try
        {
            var result = await Cli.Wrap("protontricks")
                .WithArguments(["-c", MuseDashUserInfoCommand, GameConfig.SteamAppId])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (result.ExitCode is not 0)
            {
                Logger.ZLogWarning($"Failed to query MuseDash user info from registry: {result.StandardError}");
                return null;
            }

            var hex = result.StandardOutput.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            if (hex.IsNullOrEmpty())
            {
                Logger.ZLogWarning($"Failed to read registry value from wine reg output");
                return null;
            }

            var bytes = Convert.FromHexString(hex);
            var json = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
            if (json.IsNullOrEmpty())
            {
                Logger.ZLogWarning($"MuseDash user info registry value is empty");
                return null;
            }

            Logger.ZLogInformation($"Successfully retrieved MuseDash user info from registry");
            return JsonSerializer.Deserialize(json, Default.MuseDashUidRequest);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to retrieve MuseDash user info");
            return null;
        }
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

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

    [UsedImplicitly]
    public required IResourceService ResourceService { get; init; }

    #endregion Injections
}