namespace Euterpe.Core;

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

        Logger.LogInformation("Detected game path from Steam libraryfolders.vdf: {GameFolder}", gameFolder);
        return true;
    }

    public bool TryGetGameFolderFromCommonPaths(string[] commonPaths, string relativePath, [NotNullWhen(true)] out string? gameFolder)
    {
        gameFolder = commonPaths
            .Select(x => Path.Combine(x, relativePath))
            .FirstOrDefault(Directory.Exists);

        if (gameFolder is null)
        {
            Logger.LogWarning("Failed to auto detect game path on Steam common paths");
            return false;
        }

        Logger.LogInformation("Auto detected game path on Steam common paths: {GameFolder}", gameFolder);
        return true;
    }

    #region Injections

    public required Config Config { get; init; }
    public required ILogger<GamePathService> Logger { get; init; }
    public required IVdfSerializationService VdfSerializationService { get; init; }

    #endregion Injections
}
