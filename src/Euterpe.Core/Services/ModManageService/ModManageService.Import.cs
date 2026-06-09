namespace Euterpe.Core;

internal sealed partial class ModManageService
{
    public async Task ImportModsAsync(IReadOnlyList<string> filePaths)
    {
        foreach (var filePath in filePaths)
        {
            await ImportFileAsync(filePath).ConfigureAwait(false);
        }
    }

    private async Task ImportFileAsync(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var extension = Path.GetExtension(filePath);

        var mod = extension is ".dll" or ".disabled"
            ? await ModLocalService.LoadModFromPathAsync(filePath).ConfigureAwait(false)
            : null;

        if (mod is not null)
        {
            await ImportModAsync(mod, filePath).ConfigureAwait(false);
        }
        else if (extension is ".dll")
        {
            await ImportLibAsync(filePath).ConfigureAwait(false);
        }
        else
        {
            Logger.ZLogWarning($"Ignored dropped file (not a mod or lib): {fileName}");
            NotificationService.ErrorLight(Notification_Content_Mod_Install_Failed, fileName);
        }
    }

    private async Task ImportModAsync(ModDto mod, string filePath)
    {
        var cached = _sourceCache.Lookup(mod.Name) is { HasValue: true, Value: var value } ? value : null;

        if (cached is { IsLocal: true }
            && SemVersion.Parse(mod.LocalVersion).ComparePrecedenceTo(SemVersion.Parse(cached.LocalVersion)) <= 0)
        {
            Logger.ZLogInformation($"Skipped import of {mod.Name}: version {cached.LocalVersion} already installed");
            NotificationService.WarningLight(Notification_Content_Mod_Import_Duplicated, mod.Name);
            return;
        }

        var fileName = Path.GetFileName(filePath);
        if (!FileSystemService.TryCopyFile(filePath, Path.Combine(GameConfig.ModsFolder, fileName), true))
        {
            NotificationService.ErrorLight(Notification_Content_Mod_Install_Failed, mod.Name);
            return;
        }

        if (cached is { IsLocal: true } && cached.LocalFileName != mod.LocalFileName)
        {
            FileSystemService.TryDeleteFile(Path.Combine(GameConfig.ModsFolder, cached.LocalFileName), DeleteOption.IgnoreIfNotFound);
        }

        IntegrateImportedMod(mod, cached);

        Logger.ZLogInformation($"Imported mod {mod.Name} from {fileName}");
        NotificationService.SuccessLight(Notification_Content_Mod_Install_Success, mod.Name);
    }

    private async Task ImportLibAsync(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var destPath = Path.Combine(GameConfig.UserLibsFolder, fileName);

        if (!FileSystemService.TryCopyFile(filePath, destPath, true))
        {
            NotificationService.ErrorLight(Notification_Content_Mod_Install_Failed, fileName);
            return;
        }

        var lib = await ModLocalService.LoadLibFromPathAsync(destPath).ConfigureAwait(false);
        _libsDict[lib.Name] = lib;

        Logger.ZLogInformation($"Imported lib {lib.Name} from {fileName}");
        NotificationService.SuccessLight(Notification_Content_Lib_Import_Success, lib.Name);
    }

    private void IntegrateImportedMod(ModDto localMod, ModDto? cached)
    {
        if (cached is not { HasDownloadSource: true })
        {
            _sourceCache.AddOrUpdate(localMod);
            return;
        }

        cached.FileNameWithoutExtension = localMod.FileNameWithoutExtension;
        cached.LocalVersion = localMod.LocalVersion;
        cached.IsDisabled = localMod.IsDisabled;
        cached.State = DetermineModState(localMod, cached);

        CheckConfigFile(cached);
        if (!cached.IsDisabled)
        {
            CheckLibDependencies(cached);
        }

        _sourceCache.AddOrUpdate(cached);
    }
}
