using Microsoft.Win32;
using MuseDashModTools.Models.VDFs;
using ValveKeyValue;
using ZLinq;

namespace MuseDashModTools.Core;

internal sealed partial class WindowsService
{
    private bool TryGetGameFolderForApp(LibraryFolder[] libraryFolders, string appId, string relativePath, [NotNullWhen(true)] out string? gameFolder)
    {
        if (!TryGetSteamLibraryForApp(libraryFolders, appId, out var libraryPath))
        {
            gameFolder = null;
            Logger.ZLogWarning($"AppId {appId} not found in any Steam library.");
            return false;
        }

        gameFolder = Path.Combine(libraryPath, relativePath);
        if (!Directory.Exists(gameFolder))
        {
            Logger.ZLogWarning($"Game folder not found in detected Steam library even though the appId exists there: {gameFolder}");
            return false;
        }

        Logger.ZLogInformation($"Detected game path from Steam libraryfolders.vdf: {gameFolder}");
        return true;
    }

    private bool TryGetGameFolderFromLibraries(LibraryFolder[] libraryFolders, string relativePath, [NotNullWhen(true)] out string? gameFolder)
    {
        gameFolder = libraryFolders
            .AsValueEnumerable()
            .Select(x => Path.Combine(x.Path, relativePath))
            .FirstOrDefault(Directory.Exists);

        if (gameFolder is null)
        {
            Logger.ZLogWarning($"Failed to auto detect game path from Steam libraries.");
            return false;
        }

        Logger.ZLogInformation($"Auto detected game path from Steam libraries: {gameFolder}");
        return true;
    }

    private bool TryGetGameFolderFromCommonPaths(string relativePath, [NotNullWhen(true)] out string? gameFolder)
    {
        gameFolder = WindowsPaths
            .AsValueEnumerable()
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

    private static bool TryGetSteamLibraryForApp(LibraryFolder[] libraryFolders, string appId, [NotNullWhen(true)] out string? libraryPath)
    {
        var targetLibrary = libraryFolders
            .AsValueEnumerable()
            .FirstOrDefault(library => library.Apps.ContainsKey(appId));

        libraryPath = targetLibrary?.Path;
        return targetLibrary is not null;
    }

    private bool TryGetAllSteamLibraries([NotNullWhen(true)] out LibraryFolder[]? libraryFolders)
    {
        libraryFolders = null;
        var vdfPath = Path.Combine(Config.SteamFolder, @"steamapps\libraryfolders.vdf");
        if (!File.Exists(vdfPath))
        {
            Logger.ZLogWarning($"Steam libraryfolders.vdf not found at {vdfPath}");
            return false;
        }

        try
        {
            using var stream = File.OpenRead(vdfPath);
            var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
            var data = kv.Deserialize<Dictionary<string, LibraryFolder>>(stream);

            libraryFolders = data.Values
                .AsValueEnumerable()
                .Select(library =>
                {
                    library.Path.NormalizeSlashes();
                    return library;
                })
                .ToArray();
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to deserialize libraryfolders.vdf");
            return false;
        }
    }

    private static bool TryGetSteamFolderFromRegistry(out string steamFolder)
    {
        steamFolder = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null)
            as string ?? string.Empty;
        return Directory.Exists(steamFolder);
    }
}