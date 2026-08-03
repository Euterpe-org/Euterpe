using Euterpe.Models.VDFs;

namespace Euterpe.Core;

internal sealed partial class GamePathService
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(LibraryFolder))]
    private bool TryGetAllSteamLibraries([NotNullWhen(true)] out LibraryFolder[]? libraryFolders)
    {
        libraryFolders = null;

        var vdfPath = Path.Combine(Config.SteamFolder, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(vdfPath))
        {
            Logger.LogWarning("Steam libraryfolders.vdf not found at {VdfPath}", vdfPath);
            return false;
        }

        try
        {
            var data = VdfSerializationService.DeserializeFromFile<Dictionary<string, LibraryFolder>>(vdfPath);

            libraryFolders = data.Values
                .Where(library => !library.Path.IsNullOrEmpty())
                .Select(library =>
                {
                    library.Path = library.Path.NormalizeSlashes();
                    return library;
                })
                .ToArray();
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to deserialize libraryfolders.vdf");
            return false;
        }
    }

    private bool TryGetGameFolderByAppId(LibraryFolder[] libraryFolders, string appId, string relativePath, [NotNullWhen(true)] out string? gameFolder)
    {
        gameFolder = null;

        var targetLibrary = libraryFolders.FirstOrDefault(library => library.Apps.ContainsKey(appId));
        if (targetLibrary is null)
        {
            Logger.LogWarning("AppId {AppId} not found in any Steam library", appId);
            return false;
        }

        gameFolder = Path.Combine(targetLibrary.Path, relativePath);
        if (Directory.Exists(gameFolder))
        {
            return true;
        }

        Logger.LogWarning("Game folder not found in detected Steam library even though the appId exists there: {GameFolder}", gameFolder);
        return false;
    }

    private bool TryGetGameFolderByLibraryPaths(LibraryFolder[] libraryFolders, string relativePath, [NotNullWhen(true)] out string? gameFolder)
    {
        gameFolder = libraryFolders
            .Select(x => Path.Combine(x.Path, relativePath))
            .FirstOrDefault(Directory.Exists);

        if (gameFolder is not null)
        {
            return true;
        }

        Logger.LogWarning("Game folder not found in any Steam library paths");
        return false;
    }
}
