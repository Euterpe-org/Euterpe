namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed class LinuxGamePathDiscovery : IGamePathDiscovery
{
    public bool TryGetGameFolder([NotNullWhen(true)] out string? gameFolder)
    {
        var relativePath = $"steamapps/common/{GameConfig.GameFolderName}";

        if (GamePathService.TryGetGameFolderFromVdf(GameConfig.SteamAppId, relativePath, out gameFolder))
        {
            return true;
        }

        Logger.ZLogInformation($"Could not get game folder from libraryfolders.vdf");

        if (GamePathService.TryGetGameFolderFromCommonPaths(LinuxPaths.SteamSearch, relativePath, out gameFolder))
        {
            return true;
        }

        Logger.ZLogWarning($"Failed to auto detect game path on Linux");
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

        if (!File.Exists(exePath) || !File.Exists(dllPath))
        {
            Logger.ZLogError($"{exeName} or GameAssembly.dll not found in {folderPath}");
            return false;
        }

        Logger.ZLogInformation($"{exeName} and GameAssembly.dll found in {folderPath}");
        return true;
    }

    #region Injections

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required IGamePathService GamePathService { get; init; }

    [UsedImplicitly]
    public required ILogger<LinuxGamePathDiscovery> Logger { get; init; }

    #endregion Injections
}