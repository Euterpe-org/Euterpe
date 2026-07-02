namespace Euterpe.Core;

internal sealed partial class ModManageService
{
    public async Task ImportModsAsync(IReadOnlyList<string> filePaths)
    {
        foreach (var filePath in filePaths)
        {
            await ImportFileAsync(filePath).ConfigureAwait(false);
        }

        await RefreshModStatesAsync().ConfigureAwait(false);
    }

    private async Task ImportFileAsync(string filePath)
    {
        var fileName = Path.GetFileName(filePath);

        try
        {
            if (ModFiles.IsModFile(filePath)
                && await ModLocalService.LoadModFromPathAsync(filePath).ConfigureAwait(false) is { } mod)
            {
                ImportMod(mod, filePath);
            }
            else if (Path.GetExtension(filePath) is ModFiles.DllExtension)
            {
                await ImportLibAsync(filePath).ConfigureAwait(false);
            }
            else
            {
                Logger.ZLogWarning($"Ignored dropped file (not a mod or lib): {fileName}");
                NotificationService.ErrorLight(Notification_Content_Mod_Import_Unsupported, fileName);
            }
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to import {fileName}");
            NotificationService.ErrorLight(Notification_Content_Mod_Import_Failed, fileName);
        }
    }

    private void ImportMod(ModDto mod, string filePath)
    {
        var installed = FindModByName(mod.Name) is { IsLocal: true } cached ? cached : null;

        if (installed is not null && mod.LocalVersion.ComparePrecedenceTo(installed.LocalVersion) <= 0)
        {
            Logger.ZLogInformation($"Skipped import of {mod.Name}: version {installed.LocalVersion} already installed");
            NotificationService.WarningLight(Notification_Content_Mod_Import_Duplicated, mod.Name);
            return;
        }

        if (!TryReplaceModFile(filePath, installed))
        {
            Logger.ZLogWarning($"Failed to import mod {mod.Name}: could not copy file");
            NotificationService.ErrorLight(Notification_Content_Mod_Import_Failed, mod.Name);
            return;
        }

        CacheLocalMod(mod);

        Logger.ZLogInformation($"Imported mod {mod.Name} from {Path.GetFileName(filePath)}");
        NotificationService.SuccessLight(Notification_Content_Mod_Import_Success, mod.Name);
    }

    private async Task ImportLibAsync(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var destPath = Path.Combine(GameConfig.UserLibsFolder, fileName);

        if (!FileSystemService.TryCopyFile(filePath, destPath, true))
        {
            Logger.ZLogWarning($"Failed to import library {fileName}: could not copy file");
            NotificationService.ErrorLight(Notification_Content_Lib_Import_Failed, fileName);
            return;
        }

        var lib = await ModLocalService.LoadLibFromPathAsync(destPath).ConfigureAwait(false);
        _libsDict[lib.Name] = lib;

        Logger.ZLogInformation($"Imported lib {lib.Name} from {fileName}");
        NotificationService.SuccessLight(Notification_Content_Lib_Import_Success, lib.Name);
    }

    private bool TryReplaceModFile(string sourcePath, ModDto? installed)
    {
        var fileName = Path.GetFileName(sourcePath);
        if (!FileSystemService.TryCopyFile(sourcePath, Path.Combine(GameConfig.ModsFolder, fileName), true))
        {
            return false;
        }

        if (installed is not null && installed.LocalFileName != fileName)
        {
            FileSystemService.TryDeleteFile(Path.Combine(GameConfig.ModsFolder, installed.LocalFileName), DeleteOption.IgnoreIfNotFound);
        }

        return true;
    }
}
