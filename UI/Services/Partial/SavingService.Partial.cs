using System.Globalization;
using Avalonia.Controls.ApplicationLifetimes;

namespace MuseDashModToolsUI.Services;

public sealed partial class SavingService
{
    /// <summary>
    ///     Check null setting and valid path
    /// </summary>
    private async Task CheckSettingValidity()
    {
        await NullSettingsCatch();
        await LocalService.CheckValidPathAsync();
    }

    /// <summary>
    ///     Check whether the chosen muse dash folder is valid
    /// </summary>
    /// <param name="path"></param>
    /// <returns>Is continue</returns>
    private async Task<bool> CheckValidPath(string? path)
    {
        if (path is not null)
        {
            return true;
        }

        if (!string.IsNullOrEmpty(Settings.MuseDashFolder))
        {
            Logger.Information("Path not changed");
            return false;
        }

        Logger.Error("Invalid path, showing error message box");
        await MessageBoxService.ErrorMessageBox(MsgBox_Content_InvalidPath);
        return false;
    }

    /// <summary>
    ///     Let user confirm game path and write setting
    /// </summary>
    /// <param name="folderPath"></param>
    private async Task ConfirmPath(string folderPath)
    {
        if (!await MessageBoxService.FormatNoticeConfirmMessageBox(MsgBox_Content_InstallPathConfirm, folderPath))
        {
            Logger.Information("User canceled auto detect game path, asking user to choose path");
            await OnChooseGamePathAsync();
            return;
        }

        Settings.MuseDashFolder = folderPath;
    }

    /// <summary>
    ///     Create saving folders in AppData
    /// </summary>
    private void CreateSavingFolders()
    {
        if (!Directory.Exists(Settings.CacheFolder))
        {
            Directory.CreateDirectory(Settings.CacheFolder);
        }

        if (!Directory.Exists(Settings.ChartFolder))
        {
            Directory.CreateDirectory(Settings.ChartFolder);
        }
    }

    /// <summary>
    ///     If Updater files exist, delete them
    /// </summary>
    private void DeleteUpdater()
    {
        var updateDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Update");
        var updaterPath = PlatformService.GetUpdaterFilePath(updateDirectory);
        if (!FileSystem.File.Exists(updaterPath))
        {
            return;
        }

        FileSystem.File.Delete(updaterPath);
        FileSystem.Directory.Delete(updateDirectory);
        Logger.Information("Updater found, deleting it");
    }

    /// <summary>
    ///     Get user's chosen path
    /// </summary>
    /// <returns>Chosen path</returns>
    private async Task<string?> GetChosenPath()
    {
        while (true)
        {
            if (Application.Current!.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime)
            {
                continue;
            }

            Logger.Information("Showing choose folder dialogue");

            return await FileSystemPickerService.GetSingleFolderPathAsync(FolderDialog_Title_ChooseMuseDashFolder);
        }
    }

    /// <summary>
    ///     If Settings.json exists, load it
    /// </summary>
    private void LoadSavedSetting()
    {
        if (!FileSystem.File.Exists(SettingPath))
        {
            Logger.Warning("{SettingFile} not found, skipping load", SettingFile);
            return;
        }

        Logger.Information("Found {SettingFile}, loading...", SettingFile);

        var bytes = FileSystem.File.ReadAllBytes(SettingPath);
        if (bytes is [])
        {
            return;
        }

        var savedSetting = SerializationService.DeserializeSetting(bytes);
        if (savedSetting is null)
        {
            return;
        }

        Settings.CopyFrom(savedSetting);
        _isSavedLoaded = true;
        Logger.Information("Saved setting loaded from {SettingFile}", SettingFile);
    }

    /// <summary>
    ///     Catch null setting and ask user to choose path
    ///     Set some value to default value if it's null
    /// </summary>
    private async Task NullSettingsCatch()
    {
        Logger.Information("Detecting null settings...");
        if (string.IsNullOrEmpty(Settings.MuseDashFolder))
        {
            Logger.Error("Muse Dash path is empty, asking user to choose path");
            await MessageBoxService.WarningMessageBox(MsgBox_Content_NullPath);
            await OnChooseGamePathAsync();
        }

        if (string.IsNullOrEmpty(Settings.LanguageCode))
        {
            Settings.LanguageCode = CultureInfo.CurrentUICulture.ToString();
            Logger.Warning("Language code is empty, using system language");
        }

        if (string.IsNullOrEmpty(Settings.FontName))
        {
            Settings.FontName = FontManageService.DefaultFont;
            Logger.Warning("Font name is empty, using default font");
        }

        if (string.IsNullOrEmpty(Settings.Theme))
        {
            Settings.Theme = "Dark";
            Logger.Warning("Theme is empty, using dark theme");
        }
    }

    /// <summary>
    ///     Try automatically find game folder path
    /// </summary>
    /// <returns>Is success</returns>
    private async ValueTask<bool> TryGetGameFolderPath()
    {
        Logger.Information("Trying auto detect game path...");

        if (!PlatformService.GetGamePath(out var folderPath))
        {
            return false;
        }

        await ConfirmPath(folderPath!);
        return true;
    }
}