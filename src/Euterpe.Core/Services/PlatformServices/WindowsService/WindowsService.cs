using Microsoft.Win32;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed partial class WindowsService : IPlatformService
{
    public string OsString => "win";
    public string UpdaterFileName => "Updater.exe";

    public async Task SetupDeepLinkAsync(string processPath)
    {
        try
        {
            using var schemeKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{IPlatformService.DeepLinkScheme}");
            schemeKey.SetValue(string.Empty, $"URL:{nameof(Euterpe)} Protocol", RegistryValueKind.String);
            schemeKey.SetValue("URL Protocol", string.Empty, RegistryValueKind.String);

            using var commandKey = schemeKey.CreateSubKey(@"shell\open\command");
            commandKey.SetValue(string.Empty, $"\"{processPath}\" \"%1\"", RegistryValueKind.String);

            using var iconKey = schemeKey.CreateSubKey("DefaultIcon");
            iconKey.SetValue(string.Empty, $"\"{processPath}\",0", RegistryValueKind.String);

            Logger.ZLogInformation($"Registered deep link protocol on Windows with process path: {processPath}");
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to register deep link protocol on Windows");
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
    public required ILogger<WindowsService> Logger { get; init; }

    [UsedImplicitly]
    public required IGamePathService GamePathService { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}