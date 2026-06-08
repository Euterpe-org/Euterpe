using AssetsTools.NET.Extra;

namespace Euterpe.Core;

internal sealed class GameLocalService : IGameLocalService
{
    public async Task<string> GetGameFolderAsync()
    {
        var path = string.Empty;

        while (path.IsNullOrEmpty() || !await EnsureValidGameFolderAsync(path).ConfigureAwait(true))
        {
            path = await FileSystemPickerService.GetSingleFolderPathAsync(FolderDialog_Title_ChooseMuseDashFolder).ConfigureAwait(true);
            Logger.ZLogInformation($"Selected {GameConfig.DisplayName} folder: {path}");
        }

        return path;
    }

    public async Task InstallMelonLoaderAsync()
    {
        if (!File.Exists(GameConfig.MelonLoaderZipPath))
        {
            throw new FileNotFoundException($"MelonLoader zip not found at {GameConfig.MelonLoaderZipPath}", GameConfig.MelonLoaderZipPath);
        }

        await ArchiveService.ExtractZipFileAsync(GameConfig.MelonLoaderZipPath, GameConfig.Folder).ConfigureAwait(false);

        FileSystemService.DeleteFile(GameConfig.MelonLoaderZipPath);

        Logger.ZLogInformation($"MelonLoader installed successfully");
    }

    public Task UninstallMelonLoaderAsync()
    {
        var dobbyPath = Path.Combine(GameConfig.Folder, "dobby.dll");
        var noticePath = Path.Combine(GameConfig.Folder, "NOTICE.txt");
        var versionPath = Path.Combine(GameConfig.Folder, "version.dll");

        ReadOnlySpan<string> paths = [dobbyPath, noticePath, versionPath];

        foreach (var path in paths)
        {
            FileSystemService.DeleteFile(path, DeleteOption.IgnoreIfNotFound);
        }

        FileSystemService.DeleteDirectory(GameConfig.MelonLoaderFolder, DeleteOption.IgnoreIfNotFound);

        Logger.ZLogInformation($"MelonLoader uninstalled successfully");
        return Task.CompletedTask;
    }

    public void ReadGameInformation()
    {
        var assetsManager = new AssetsManager();
        assetsManager.LoadClassPackage(ResourceService.GetAssetAsStream("classdata.tpk"));

        var instance = assetsManager.LoadAssetsFile(GameConfig.GlobalGameManagersPath, true);
        var unityVersion = instance.file.Metadata.UnityVersion;
        assetsManager.LoadClassDatabaseFromPackage(unityVersion);
        var playerSettings = instance.file.GetAssetsOfType(AssetClassID.PlayerSettings)[0];
        var bundleVersion = assetsManager.GetBaseField(instance, playerSettings)["bundleVersion"].AsString;

        GameConfig.GameVersion = bundleVersion;
        GameConfig.UnityVersion = unityVersion[..^2];

        Logger.ZLogInformation($"Game information read successfully - Game version: {bundleVersion}, Unity version: {unityVersion}");
        assetsManager.UnloadAll();
    }

    public void ReadMelonLoaderVersion()
    {
        ReadOnlySpan<string> paths =
        [
            Path.Combine(GameConfig.Folder, "version.dll"),
            Path.Combine(GameConfig.MelonLoaderFolder, "net6", "MelonLoader.dll"),
            Path.Combine(GameConfig.MelonLoaderFolder, "MelonLoader.dll")
        ];

        foreach (var path in paths)
        {
            if (!File.Exists(path) || ReadFileVersion(path) is not { } version)
            {
                continue;
            }

            GameConfig.MelonLoaderVersion = Version.Parse(version).ToString(3);
            Logger.ZLogInformation($"MelonLoader version detected: {GameConfig.MelonLoaderVersion}");
            return;
        }

        Logger.ZLogInformation($"MelonLoader not installed");
    }

    private async ValueTask<bool> EnsureValidGameFolderAsync(string folderPath)
    {
        if (GamePaths.CheckIsValidGameFolder(folderPath))
        {
            return true;
        }

        await MessageBoxService.ErrorAsync(MessageBox_Content_InvalidPath).ConfigureAwait(true);
        return false;
    }

    private static string? ReadFileVersion(string filePath)
    {
        var versionInfo = FileVersionInfo.GetVersionInfo(filePath);
        return versionInfo.FileVersion;
    }

    #region Injections

    public required IArchiveService ArchiveService { get; init; }
    public required IFileSystemPickerService FileSystemPickerService { get; init; }
    public required IFileSystemService FileSystemService { get; init; }
    public required GameConfig GameConfig { get; init; }
    public required IGamePathDiscovery GamePaths { get; init; }
    public required ILogger<GameLocalService> Logger { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }
    public required IResourceService ResourceService { get; init; }

    #endregion Injections
}