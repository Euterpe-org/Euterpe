using System.Reflection;
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
    private const string DeepLinkDesktopFileName = "com.euterpe-org.Euterpe.desktop";
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
            var applicationsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "applications");
            var desktopFilePath = Path.Combine(applicationsPath, DeepLinkDesktopFileName);
            Directory.CreateDirectory(applicationsPath);
            var execCommand = BuildDeepLinkExecCommand(processPath);

            var content =
                $"""
                 [Desktop Entry]
                 Type=Application
                 Name=Euterpe
                 Exec={execCommand} %u
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

    public async Task<MuseDashUidRequest?> GetMuseDashUserIdAsync()
    {
        try
        {
            var result = await Cli.Wrap("protontricks")
                .WithArguments(["-c", MuseDashUserInfoCommand, GameConstants.MuseDashSteamAppId])
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

    private string BuildDeepLinkExecCommand(string processPath)
    {
        if (!Path.GetFileName(processPath).StartsWith("dotnet", StringComparison.OrdinalIgnoreCase))
        {
            return $"\"{processPath.EscapeDesktopExecArgument()}\"";
        }

        var entryAssemblyPath = Assembly.GetEntryAssembly()?.Location;
        if (entryAssemblyPath.IsNullOrEmpty())
        {
            Logger.ZLogWarning($"Entry assembly location is unavailable, falling back to process path for deep link registration");
            return $"\"{processPath.EscapeDesktopExecArgument()}\"";
        }

        Logger.ZLogInformation($"Detected dotnet host on Linux, registering deep link with entry assembly path: {entryAssemblyPath}");
        return $"\"{processPath.EscapeDesktopExecArgument()}\" \"{entryAssemblyPath.EscapeDesktopExecArgument()}\"";
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