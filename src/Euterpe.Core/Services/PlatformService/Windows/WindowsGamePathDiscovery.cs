namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class WindowsGamePathDiscovery : IGamePathDiscovery
{
    public bool TryGetGameFolder([NotNullWhen(true)] out string? gameFolder)
    {
        var relativePath = Path.Combine("steamapps", "common", GameConfig.GameFolderName);

        if (GamePathService.TryGetGameFolderFromVdf(GameConfig.SteamAppId, relativePath, out gameFolder))
        {
            return true;
        }

        Logger.ZLogInformation($"Could not get game folder from libraryfolders.vdf");

        if (GamePathService.TryGetGameFolderFromCommonPaths(WindowsPaths.SteamSearch, relativePath, out gameFolder))
        {
            return true;
        }

        Logger.ZLogWarning($"Failed to auto detect game path on Windows");
        return false;
    }

    public bool CheckIsValidGameFolder([NotNullWhen(true)] string? folderPath)
    {
        if (folderPath.IsNullOrEmpty())
        {
            return false;
        }

        var exeName = GameConfig.ExecutableName;
        var exePath = Path.Combine(folderPath, exeName);
        var dllPath = Path.Combine(folderPath, "GameAssembly.dll");

        if (File.Exists(exePath) && File.Exists(dllPath))
        {
            Logger.ZLogInformation($"{exeName} and GameAssembly.dll found in {folderPath}");
            return true;
        }

        Logger.ZLogError($"{exeName} or GameAssembly.dll not found in {folderPath}");
        return false;
    }

    #region Injections

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required IGamePathService GamePathService { get; init; }

    [UsedImplicitly]
    public required ILogger<WindowsGamePathDiscovery> Logger { get; init; }

    #endregion Injections
}