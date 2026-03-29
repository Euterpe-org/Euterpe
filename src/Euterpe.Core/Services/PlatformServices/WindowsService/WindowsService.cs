using System.Text;
using System.Text.Json;
using Euterpe.Contracts.Account;
using Microsoft.Win32;
using static Euterpe.Core.JsonContexts.SnakeCaseJsonContext;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed partial class WindowsService : IPlatformService
{
    private const string MuseDashRegistrySubKey = @"Software\PeroPeroGames\MuseDash";
    private const string UserInfoValueName = "peropero_account_user_info_h3003705636";

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

    public async Task<MuseDashUidRequest?> GetMuseDashUserIdAsync()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(MuseDashRegistrySubKey, false);
            var value = key?.GetValue(UserInfoValueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);

            if (value is not byte[] bytes || bytes is [])
            {
                Logger.ZLogWarning($"MuseDash user info registry value is missing or invalid");
                return null;
            }

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
            Logger.ZLogError(ex, $"Failed to get MuseDash user ID from registry");
            return null;
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