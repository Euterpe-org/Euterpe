using AsmResolver.DotNet;
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

    public string[] GetModFilePaths() => Directory.EnumerateFiles(GameConfig.ModsFolder)
        .Where(x => Path.GetExtension(x) is ".disabled" || Path.GetExtension(x) is ".dll")
        .ToArray();

    public string[] GetLibFilePaths() => Directory.EnumerateFiles(GameConfig.UserLibsFolder)
        .Where(x => Path.GetExtension(x) is ".dll")
        .ToArray();

    public async Task InstallMelonLoaderAsync()
    {
        if (!FileSystemService.CheckFileExists(GameConfig.MelonLoaderZipPath))
        {
            throw new InvalidOperationException($"MelonLoader zip not found at {GameConfig.MelonLoaderZipPath}");
        }

        if (!await ArchiveService.ExtractZipFileAsync(GameConfig.MelonLoaderZipPath, GameConfig.Folder).ConfigureAwait(false))
        {
            throw new InvalidOperationException($"Failed to extract MelonLoader zip at {GameConfig.MelonLoaderZipPath}");
        }

        if (!FileSystemService.TryDeleteFile(GameConfig.MelonLoaderZipPath))
        {
            throw new InvalidOperationException($"Failed to delete MelonLoader zip at {GameConfig.MelonLoaderZipPath}");
        }

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
            if (!FileSystemService.TryDeleteFile(path, DeleteOption.IgnoreIfNotFound))
            {
                throw new InvalidOperationException($"Failed to delete {path}");
            }
        }

        if (!FileSystemService.TryDeleteDirectory(GameConfig.MelonLoaderFolder, DeleteOption.IgnoreIfNotFound))
        {
            throw new InvalidOperationException($"Failed to delete MelonLoader folder at {GameConfig.MelonLoaderFolder}");
        }

        Logger.ZLogInformation($"MelonLoader uninstalled successfully");
        return Task.CompletedTask;
    }

    public async Task<ModDto?> LoadModFromPathAsync(string filePath)
    {
        var mod = new ModDto
        {
            FileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath),
            IsDisabled = Path.GetExtension(filePath) is ".disabled"
        };

        try
        {
            var bytes = await File.ReadAllBytesAsync(filePath).ConfigureAwait(false);
            var assembly = AssemblyDefinition.FromBytes(bytes);

            var attribute = assembly.FindCustomAttributes("MelonLoader", "MelonInfoAttribute").FirstOrDefault();
            if (attribute is null)
            {
                Logger.ZLogWarning($"{filePath} is not a mod file but inside Mods folder");
                return null;
            }

            mod.Name = attribute.Signature!.FixedArguments[1].ToString();
            mod.LocalVersion = attribute.Signature!.FixedArguments[2].ToString();
            mod.Author = attribute.Signature!.FixedArguments[3].ToString();
            mod.SHA256 = SHA256Utils.HexLowerFromBytes(bytes);

            return mod;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to load mod from {filePath}, skipping");
            return null;
        }
    }

    public async Task<LibDto> LoadLibFromPathAsync(string filePath) =>
        new()
        {
            Name = Path.GetFileNameWithoutExtension(filePath),
            FileName = Path.GetFileName(filePath),
            SHA256 = await SHA256Utils.HexLowerFromPathAsync(filePath).ConfigureAwait(false),
            IsLocal = true
        };

    public async ValueTask ReadGameInformationAsync()
    {
        var assetsManager = new AssetsManager();
        assetsManager.LoadClassPackage(ResourceService.GetAssetAsStream("classdata.tpk"));
        var bundlePath = Path.Combine(GameConfig.Folder, GameConfig.GameDataFolderName, "globalgamemanagers");
        try
        {
            var instance = assetsManager.LoadAssetsFile(bundlePath, true);
            var unityVersion = instance.file.Metadata.UnityVersion;
            assetsManager.LoadClassDatabaseFromPackage(unityVersion);
            var playerSettings = instance.file.GetAssetsOfType(AssetClassID.PlayerSettings)[0];
            var bundleVersion = assetsManager.GetBaseField(instance, playerSettings)["bundleVersion"].AsString;

            GameConfig.GameVersion = bundleVersion;
            GameConfig.UnityVersion = unityVersion[..^2];

            Logger.ZLogInformation($"Game information read successfully - Game version: {bundleVersion}, Unity version: {unityVersion}");
            assetsManager.UnloadAll();
        }
        catch (Exception ex)
        {
            Logger.ZLogCritical(ex, $"Read game information failed");
            await MessageBoxService.ErrorAsync(MessageBox_Content_ReadGameInformation_Failed, bundlePath).ConfigureAwait(true);
            Environment.Exit(0);
        }
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

    [UsedImplicitly]
    public required IArchiveService ArchiveService { get; init; }

    [UsedImplicitly]
    public required IFileSystemPickerService FileSystemPickerService { get; init; }

    [UsedImplicitly]
    public required IFileSystemService FileSystemService { get; init; }

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required IGamePathDiscovery GamePaths { get; init; }

    [UsedImplicitly]
    public required ILogger<GameLocalService> Logger { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public required IResourceService ResourceService { get; init; }

    #endregion Injections
}