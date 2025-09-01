namespace MuseDashModTools.Core;

internal sealed partial class GamePathService : IGamePathService
{
    public bool TryGetGameFolderFromVdf(string appId, string relativePath, [NotNullWhen(true)] out string? gameFolder)
    {
        gameFolder = null;

        if (!TryGetAllSteamLibraries(out var libraryFolders))
        {
            return false;
        }

        if (!TryGetGameFolderByAppId(libraryFolders, appId, relativePath, out gameFolder) &&
            !TryGetGameFolderByLibraryPaths(libraryFolders, relativePath, out gameFolder))
        {
            return false;
        }

        Logger.ZLogInformation($"Detected game path from Steam libraryfolders.vdf: {gameFolder}");
        return true;
    }

    public bool TryGetGameFolderFromCommonPaths(string[] commonPaths, string relativePath, [NotNullWhen(true)] out string? gameFolder)
    {
        gameFolder = commonPaths
            .Select(x => Path.Combine(x, relativePath))
            .FirstOrDefault(Directory.Exists);

        if (gameFolder is null)
        {
            Logger.ZLogWarning($"Failed to auto detect game path on Steam common paths.");
            return false;
        }

        Logger.ZLogInformation($"Auto detected game path on Steam common paths: {gameFolder}");
        return true;
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required ILogger<GamePathService> Logger { get; init; }

    [UsedImplicitly]
    public required IVdfSerializationService VdfSerializationService { get; init; }

    #endregion Injections
}