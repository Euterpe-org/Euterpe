using System.Globalization;
using System.Text.Json.Nodes;
using Avalonia.Controls.ApplicationLifetimes;
using NuGet.Versioning;

namespace MuseDashModToolsUI.Services;

public partial class SavingService
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
        if (path is not null) return true;

        if (!string.IsNullOrEmpty(Settings.MuseDashFolder))
        {
            _logger.Information("Path not changed");
            return false;
        }

        _logger.Error("Invalid path, showing error message box");
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
            _logger.Information("User canceled auto detect game path, asking user to choose path");
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
        if (!Directory.Exists(ConfigFolderPath))
            Directory.CreateDirectory(ConfigFolderPath);
        if (!Directory.Exists(ChartFolderPath))
            Directory.CreateDirectory(ChartFolderPath);
    }

    /// <summary>
    ///     If Updater files exist, delete them
    /// </summary>
    private void DeleteUpdater()
    {
        var updateDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Update");
        var updaterPath = _platformService.GetUpdaterFilePath(updateDirectory);
        if (!_fileSystem.File.Exists(updaterPath)) return;

        _fileSystem.File.Delete(updaterPath);
        _fileSystem.Directory.Delete(updateDirectory);
        _logger.Information("Updater found, deleting it");
    }

    /// <summary>
    ///     Get user's chosen path
    /// </summary>
    /// <returns>Chosen path</returns>
    private async Task<string?> GetChosenPath()
    {
        while (true)
        {
            if (Application.Current!.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime) continue;
            _logger.Information("Showing choose folder dialogue");

            return await FileSystemPickerService.GetSingleFolderPathAsync(FolderDialog_Title_ChooseMuseDashFolder);
        }
    }

    /// <summary>
    ///     Load saved setting from Settings.json
    ///     Delete Updater
    /// </summary>
    private async Task Load()
    {
        await LoadSavedSetting();
        CreateSavingFolders();
        DeleteUpdater();
    }

    /// <summary>
    ///     If Settings.json exists, load it
    /// </summary>
    private async Task LoadSavedSetting()
    {
        if (!_fileSystem.File.Exists(SettingPath))
        {
            _logger.Warning("Settings.json not found, skipping load");
            return;
        }

        var text = await _fileSystem.File.ReadAllTextAsync(SettingPath);
        var settings = JsonNode.Parse(text);
        if (settings is null) return;

        Settings.MuseDashFolder = settings["MuseDashFolder"]?.ToString();
        Settings.LanguageCode = settings["LanguageCode"]?.ToString() ?? CultureInfo.CurrentUICulture.ToString();
        Settings.FontName = settings["FontName"]?.ToString() ?? FontManageService.DefaultFont;
        if (SemanticVersion.TryParse(settings["SkipVersion"]?.ToString()!, out var version))
            Settings.SkipVersion = version;
        Settings.DownloadSource = Enum.Parse<DownloadSources>(settings["DownloadSource"]?.ToString()!);
        Settings.DownloadPrerelease = bool.Parse(settings["DownloadPrerelease"]?.ToString()!);
        Settings.CustomDownloadSource = settings["CustomDownloadSource"]?.ToString();
        Settings.AskInstallMuseDashModTools = Enum.Parse<AskType>(settings["AskInstallMuseDashModTools"]?.ToString()!);
        Settings.AskEnableDependenciesWhenInstalling = Enum.Parse<AskType>(settings["AskEnableDependenciesWhenInstalling"]?.ToString()!);
        Settings.AskEnableDependenciesWhenEnabling = Enum.Parse<AskType>(settings["AskEnableDependenciesWhenEnabling"]?.ToString()!);
        Settings.AskDisableDependenciesWhenDeleting = Enum.Parse<AskType>(settings["AskDisableDependenciesWhenDeleting"]?.ToString()!);
        Settings.AskDisableDependenciesWhenDisabling = Enum.Parse<AskType>(settings["AskDisableDependenciesWhenDisabling"]?.ToString()!);

        _isSavedLoaded = true;
        _logger.Information("Saved setting loaded from Settings.json");
    }

    /// <summary>
    ///     Catch null setting and ask user to choose path
    ///     Set some value to default value if it's null
    /// </summary>
    private async Task NullSettingsCatch()
    {
        _logger.Information("Detecting null settings...");
        if (string.IsNullOrEmpty(Settings.MuseDashFolder))
        {
            _logger.Error("Muse Dash path is empty, asking user to choose path");
            await MessageBoxService.WarningMessageBox(MsgBox_Content_NullPath);
            await OnChooseGamePathAsync();
        }

        if (string.IsNullOrEmpty(Settings.LanguageCode))
        {
            Settings.LanguageCode = CultureInfo.CurrentUICulture.ToString();
            _logger.Warning("Language code is empty, using system language");
        }

        if (string.IsNullOrEmpty(Settings.FontName))
        {
            Settings.FontName = FontManageService.DefaultFont;
            _logger.Warning("Font name is empty, using default font");
        }
    }

    /// <summary>
    ///     Try automatically find game folder path
    /// </summary>
    /// <returns>Is success</returns>
    private async Task<bool> TryGetGameFolderPath()
    {
        _logger.Information("Trying auto detect game path...");

        if (!_platformService.GetGamePath(out var folderPath)) return false;
        await ConfirmPath(folderPath!);
        return true;
    }
}