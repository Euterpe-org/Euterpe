using AsmResolver.DotNet;
using AssetsTools.NET.Extra;
using CliWrap;
using CliWrap.Buffered;

namespace MuseDashModTools.Core;

internal sealed partial class LocalService : ILocalService
{
    public async Task<bool> CheckDotNetRuntimeInstalledAsync()
    {
        try
        {
            var result = await Cli.Wrap("dotnet")
                .WithArguments("--list-runtimes")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            return result.IsSuccess && result.StandardOutput.Contains("Microsoft.NETCore.App 6.");
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to check .NET runtime installation");
            return false;
        }
    }

    public async Task<bool> CheckDotNetSdkInstalledAsync()
    {
        try
        {
            var result = await Cli.Wrap("dotnet")
                .WithArguments("--list-sdks")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            return result.IsSuccess && !result.StandardOutput.IsNullOrEmpty();
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to check .NET SDK installation");
            return false;
        }
    }

    public async Task<bool> CheckModTemplateInstalledAsync()
    {
        try
        {
            var result = await Cli.Wrap("dotnet")
                .WithArguments(["new", "list", "musedashmod"])
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync()
                .ConfigureAwait(false);

            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to check Mod Template installation");
            return false;
        }
    }

    public async Task<string> GetSteamFolderAsync()
    {
        var path = string.Empty;

        while (path.IsNullOrEmpty() || !await EnsureValidSteamFolderAsync(path).ConfigureAwait(true))
        {
            path = await FileSystemPickerService.GetSingleFolderPathAsync(FolderDialog_Title_ChooseSteamFolder).ConfigureAwait(true);
            Logger.ZLogInformation($"Selected Steam folder: {path}");
        }

        return path;
    }

    public async Task<string> GetSteamExecPathAsync()
    {
        var path = string.Empty;

        while (path.IsNullOrEmpty() || !await EnsureValidSteamExecPathAsync(path).ConfigureAwait(true))
        {
            path = await FileSystemPickerService.GetSingleFilePathAsync(FileDialog_Title_ChooseSteamExec).ConfigureAwait(true);
            Logger.ZLogInformation($"Selected Steam executable: {path}");
        }

        return path;
    }

    public async Task<string> GetMuseDashFolderAsync()
    {
        var path = string.Empty;

        while (path.IsNullOrEmpty() || !await EnsureValidGameFolderAsync(path).ConfigureAwait(true))
        {
            path = await FileSystemPickerService.GetSingleFolderPathAsync(FolderDialog_Title_ChooseMuseDashFolder).ConfigureAwait(true);
            Logger.ZLogInformation($"Selected MuseDash folder: {path}");
        }

        return path;
    }

    public async Task<string> GetCacheFolderAsync()
    {
        var path = string.Empty;
        while (path.IsNullOrEmpty())
        {
            path = await FileSystemPickerService.GetSingleFolderPathAsync(FolderDialog_Title_ChooseCacheFolder).ConfigureAwait(true);
            Logger.ZLogInformation($"Selected Cache folder: {path}");
        }

        return path;
    }

    public string[] GetModFilePaths() => Directory.EnumerateFiles(Config.ModsFolder)
        .Where(x => Path.GetExtension(x) is ".disabled" || Path.GetExtension(x) is ".dll")
        .ToArray();

    public string[] GetLibFilePaths() => Directory.EnumerateFiles(Config.UserLibsFolder)
        .Where(x => Path.GetExtension(x) is ".dll")
        .ToArray();

    public async Task<bool> InstallMelonLoaderAsync()
    {
        if (!FileSystemService.CheckFileExists(Config.MelonLoaderZipPath))
        {
            await MessageBoxService.ErrorAsync("MelonLoader zip file not found").ConfigureAwait(false);
            return false;
        }

        if (!await ArchiveService.ExtractZipFileAsync(Config.MelonLoaderZipPath, Config.MuseDashFolder).ConfigureAwait(true))
        {
            await MessageBoxService.ErrorAsync("Failed to unzip MelonLoader").ConfigureAwait(false);
            return false;
        }

        if (!FileSystemService.TryDeleteFile(Config.MelonLoaderZipPath))
        {
            await MessageBoxService.ErrorAsync(MessageBox_Content_MelonLoader_DeleteZip_Failed, Config.MelonLoaderZipPath).ConfigureAwait(false);
            return false;
        }

        Logger.ZLogInformation($"MelonLoader installed successfully");
        await MessageBoxService.SuccessAsync(MessageBox_Content_MelonLoader_Install_Success).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> UninstallMelonLoaderAsync()
    {
        var dobbyPath = Path.Combine(Config.MuseDashFolder, "dobby.dll");
        var noticePath = Path.Combine(Config.MuseDashFolder, "NOTICE.txt");
        var versionPath = Path.Combine(Config.MuseDashFolder, "version.dll");

        foreach (var path in new[] { dobbyPath, noticePath, versionPath })
        {
            if (FileSystemService.TryDeleteFile(path, DeleteOption.IgnoreIfNotFound))
            {
                continue;
            }

            await MessageBoxService.ErrorAsync($"Failed to delete {Path.GetFileName(path)}").ConfigureAwait(true);
            return false;
        }

        if (!FileSystemService.TryDeleteDirectory(Config.MelonLoaderFolder, DeleteOption.IgnoreIfNotFound))
        {
            await MessageBoxService.ErrorAsync("Failed to delete MelonLoader folder").ConfigureAwait(true);
            return false;
        }

        Logger.ZLogInformation($"MelonLoader uninstalled successfully");
        await MessageBoxService.SuccessAsync("MelonLoader uninstalled successfully").ConfigureAwait(true);
        return true;
    }

    public async Task<ModDto?> LoadModFromPathAsync(string filePath)
    {
        var mod = new ModDto
        {
            FileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath),
            IsDisabled = Path.GetExtension(filePath) is ".disabled"
        };

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
        var bundlePath = Path.Combine(Config.MuseDashFolder, "MuseDash_Data", "globalgamemanagers");
        try
        {
            var instance = assetsManager.LoadAssetsFile(bundlePath, true);
            var unityVersion = instance.file.Metadata.UnityVersion;
            assetsManager.LoadClassDatabaseFromPackage(unityVersion);
            var playerSettings = instance.file.GetAssetsOfType(AssetClassID.PlayerSettings)[0];
            var bundleVersion = assetsManager.GetBaseField(instance, playerSettings)["bundleVersion"].AsString;

            Config.GameVersion = bundleVersion;
            Config.UnityVersion = unityVersion[..^2];

            Logger.ZLogInformation($"Game information read successfully - Game version: {bundleVersion}, Unity version: {unityVersion}");
            assetsManager.UnloadAll();
        }
        catch (Exception ex)
        {
            Logger.ZLogCritical(ex, $"Read game information failed");
            await MessageBoxService.ErrorAsync("Reading Game Information failed", bundlePath).ConfigureAwait(true);
            Environment.Exit(0);
        }
    }

    public void ReadMelonLoaderVersion()
    {
        ReadOnlySpan<string> paths =
        [
            Path.Combine(Config.MuseDashFolder, "version.dll"),
            Path.Combine(Config.MelonLoaderFolder, "net6", "MelonLoader.dll"),
            Path.Combine(Config.MelonLoaderFolder, "MelonLoader.dll")
        ];

        foreach (var path in paths)
        {
            if (!File.Exists(path) || ReadFileVersion(path) is not { } version)
            {
                continue;
            }

            Config.MelonLoaderVersion = version[..^2];
        }

        Logger.ZLogInformation($"MelonLoader not installed");
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required IArchiveService ArchiveService { get; init; }

    [UsedImplicitly]
    public required IFileSystemService FileSystemService { get; init; }

    [UsedImplicitly]
    public required IFileSystemPickerService FileSystemPickerService { get; init; }

    [UsedImplicitly]
    public required ILogger<LocalService> Logger { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    [UsedImplicitly]
    public required IResourceService ResourceService { get; init; }

    #endregion Injections
}