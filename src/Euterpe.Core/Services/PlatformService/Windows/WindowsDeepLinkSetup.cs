using Microsoft.Win32;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class WindowsDeepLinkSetup : IDeepLinkSetup
{
    #region Injections

    public required ILogger<WindowsDeepLinkSetup> Logger { get; init; }

    #endregion Injections

    public async Task SetupDeepLinkAsync(string processPath)
    {
        try
        {
            using var schemeKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{IDeepLinkSetup.DeepLinkScheme}");
            schemeKey.SetValue(string.Empty, $"URL:{AppName} Protocol", RegistryValueKind.String);
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
}
