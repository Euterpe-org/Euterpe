using System.IO.Compression;
using System.Text;
using AsmResolver.DotNet;
using AssetsTools.NET.Extra;
using CliWrap;

namespace MuseDashModTools.Core;

internal sealed partial class LocalService : ILocalService
{
    public async Task CheckDotNetRuntimeInstalledAsync()
    {
        var outputStringBuilder = new StringBuilder();
        await Cli.Wrap("dotnet")
            .WithArguments("--list-runtimes")
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(outputStringBuilder))
            .ExecuteAsync()
            .ConfigureAwait(false);

        if (!outputStringBuilder.ToString().Contains("Microsoft.WindowsDesktop.App 6."))
        {
            Logger.ZLogInformation($"DotNet Runtime not found, showing error message box...");
            await MessageBoxService.ErrorAsync(MessageBox_Content_DotNetRuntimeNotFound).ConfigureAwait(true);
        }
    }

    public async Task CheckDotNetSdkInstalledAsync()
    {
        var outputStringBuilder = new StringBuilder();
        await Cli.Wrap("dotnet")
            .WithArguments("--list-sdks")
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(outputStringBuilder))
            .ExecuteAsync()
            .ConfigureAwait(false);

        if (outputStringBuilder.ToString().IsNullOrEmpty())
        {
            Logger.ZLogInformation($"DotNet SDK not found, showing error message box...");
            await MessageBoxService.ErrorAsync(MessageBox_Content_DotNetSDKNotFound).ConfigureAwait(true);
        }
    }

    public Task InstallDotNetRuntimeAsync() => throw new NotImplementedException();

    public Task InstallDotNetSdkAsync() => throw new NotImplementedException();

    public string[] GetModFilePaths() => Directory.EnumerateFiles(Config.ModsFolder)
        .Where(x => Path.GetExtension(x) == ".disabled" || Path.GetExtension(x) == ".dll")
        .ToArray();

    public string[] GetLibFilePaths() => Directory.EnumerateFiles(Config.UserLibsFolder)
        .Where(x => Path.GetExtension(x) == ".dll")
        .ToArray();

    public async Task<bool> InstallMelonLoaderAsync()
    {
        if (!FileSystemService.CheckFileExists(Config.MelonLoaderZipPath))
        {
            await MessageBoxService.ErrorAsync("MelonLoader zip file not found").ConfigureAwait(false);
            return false;
        }

        if (!ExtractZipFile(Config.MelonLoaderZipPath, Config.MuseDashFolder))
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

    public async Task<string> GetMuseDashFolderAsync()
    {
        var path = string.Empty;

        while (path.IsNullOrEmpty() || !await CheckValidPathAsync(path).ConfigureAwait(true))
        {
            path = await FileSystemPickerService.GetSingleFolderPathAsync(FolderDialog_Title_ChooseMuseDashFolder).ConfigureAwait(true);
            Logger.ZLogInformation($"Selected MuseDash folder: {path}");
        }

        return path;
    }

    public async Task<ModDto?> LoadModFromPathAsync(string filePath)
    {
        var mod = new ModDto
        {
            FileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath),
            IsDisabled = Path.GetExtension(filePath) == ".disabled"
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
        mod.SHA256 = HashUtils.ComputeSHA256HashFromBytes(bytes);

        return mod;
    }

    public async Task<LibDto> LoadLibFromPathAsync(string filePath) =>
        new()
        {
            Name = Path.GetFileNameWithoutExtension(filePath),
            FileName = Path.GetFileName(filePath),
            SHA256 = await HashUtils.ComputeSHA256HashFromPathAsync(filePath).ConfigureAwait(false),
            IsLocal = true
        };

    public async ValueTask<string> ReadGameVersionAsync()
    {
        var assetsManager = new AssetsManager();
        assetsManager.LoadClassPackage(ResourceService.GetAssetAsStream("classdata.tpk"));
        var bundlePath = Path.Combine(Config.MuseDashFolder, "MuseDash_Data", "globalgamemanagers");
        try
        {
            var instance = assetsManager.LoadAssetsFile(bundlePath, true);
            assetsManager.LoadClassDatabaseFromPackage(instance.file.Metadata.UnityVersion);
            var playerSettings = instance.file.GetAssetsOfType(AssetClassID.PlayerSettings)[0];
            var bundleVersion = assetsManager.GetBaseField(instance, playerSettings)["bundleVersion"].AsString;

            Logger.ZLogInformation($"Game version read successfully: {bundleVersion}");
            assetsManager.UnloadAll();
            return bundleVersion;
        }
        catch (Exception ex)
        {
            Logger.ZLogCritical(ex, $"Read game version failed, showing error message box...");
            await MessageBoxService.ErrorAsync("Reading Game Version failed", bundlePath).ConfigureAwait(true);
            Environment.Exit(0);
        }

        return string.Empty;
    }

    public string? ReadMelonLoaderVersion()
    {
        ReadOnlySpan<string> paths =
        [
            Path.Combine(Config.MelonLoaderFolder, "net6", "MelonLoader.dll"),
            Path.Combine(Config.MelonLoaderFolder, "MelonLoader.dll")
        ];

        foreach (var path in paths)
        {
            if (!File.Exists(path))
            {
                continue;
            }

            return ReadFileVersion(path);
        }

        Logger.ZLogInformation($"MelonLoader.dll not found");
        return null;
    }

    public bool ExtractZipFile(string zipPath, string extractPath)
    {
        try
        {
            ZipFile.ExtractToDirectory(zipPath, extractPath, true);
            Logger.ZLogInformation($"Successfully extracted {zipPath} to {extractPath}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to extract file {zipPath} to {extractPath}");
            return false;
        }
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required IFileSystemService FileSystemService { get; init; }

    [UsedImplicitly]
    public required IFileSystemPickerService FileSystemPickerService { get; init; }

    [UsedImplicitly]
    public required ILogger<LocalService> Logger { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public required IResourceService ResourceService { get; init; }

    #endregion Injections
}