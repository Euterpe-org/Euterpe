using System.Globalization;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using MuseDashModToolsUI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;

namespace MuseDashModToolsUI.Services;

public partial class SavingService
{
    /// <summary>
    ///     Try automatically find game folder path
    /// </summary>
    private async Task TryGetGameFolderPath()
    {
        _logger.Information("Trying auto detect game path");
        if (OperatingSystem.IsWindows() && LocalService.Value.GetPathOnWindows(out var folderPath)) await ConfirmPath(folderPath!);
        else if (OperatingSystem.IsLinux() && LocalService.Value.GetPathOnLinux(out folderPath)) await ConfirmPath(folderPath!);
    }

    /// <summary>
    ///     Let user confirm game path and write setting
    /// </summary>
    /// <param name="folderPath"></param>
    private async Task ConfirmPath(string folderPath)
    {
        if (!await MessageBoxService.NoticeConfirmMessageBox(MsgBox_Title_Notice, string.Format(MsgBox_Content_InstallPathConfirm, folderPath)))
        {
            await MessageBoxService.WarningMessageBox(MsgBox_Content_ChoosePath);
            await OnChoosePath();
            return;
        }

        await WriteSettings(folderPath);
    }

    /// <summary>
    ///     Write Setting into Settings.json
    /// </summary>
    /// <param name="path">Muse Dash Folder Path</param>
    private async Task WriteSettings(string path)
    {
        Settings.MuseDashFolder = path;
        Settings.LanguageCode ??= CultureInfo.CurrentUICulture.Name;

        SettingsViewModel.Value.UpdatePath();

        var json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
        await _fileSystem.File.WriteAllTextAsync(SettingPath, json);
        _logger.Information("Settings saved to Settings.json");
    }

    /// <summary>
    ///     Catch null setting and ask user to choose path
    ///     Set some value to default value if it's null
    /// </summary>
    /// <param name="settings"></param>
    private async Task NullSettingCatch(Setting settings)
    {
        if (string.IsNullOrEmpty(settings.MuseDashFolder))
        {
            _logger.Error("Settings.json stored path is empty, asking user to choose path");
            await MessageBoxService.WarningMessageBox(MsgBox_Content_NullPath);
            await OnChoosePath();
            settings.MuseDashFolder = Settings.MuseDashFolder;
        }

        if (string.IsNullOrEmpty(settings.LanguageCode))
        {
            settings.LanguageCode = CultureInfo.CurrentUICulture.ToString();
            _logger.Warning("Settings.json stored language code is empty, using system language");
        }

        if (string.IsNullOrEmpty(settings.FontName))
        {
            settings.FontName = FontManageService.DefaultFont;
            _logger.Warning("Settings.json stored font name is empty, using default font");
        }
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

            var dialogue = await new Window().StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                { AllowMultiple = false, Title = FolderDialog_Title });

            if (dialogue.Count != 0) return dialogue[0].TryGetLocalPath();
            if (!string.IsNullOrEmpty(Settings.MuseDashFolder)) return Settings.MuseDashFolder;

            _logger.Error("Invalid path, showing error message box");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_InvalidPath);
        }
    }

    /// <summary>
    ///     Load saved setting from Settings.json
    ///     Delete Updater
    /// </summary>
    private async Task Load()
    {
        await LoadSavedSetting();
        DeleteUpdater();
    }

    /// <summary>
    ///     If Settings.json exists, load it
    /// </summary>
    private async Task LoadSavedSetting()
    {
        if (!_fileSystem.File.Exists(SettingPath)) return;
        var text = await _fileSystem.File.ReadAllTextAsync(SettingPath);
        var settings = JObject.Parse(text);

        Settings.MuseDashFolder = settings["MuseDashFolder"]?.ToString();
        Settings.LanguageCode = settings["LanguageCode"]?.ToString();
        Settings.FontName = settings["FontName"]?.ToString();
        if (SemanticVersion.TryParse(settings["SkipVersion"]?.ToString()!, out var version))
            Settings.SkipVersion = version;
        Settings.DownloadSource = Enum.Parse<DownloadSources>(settings["DownloadSource"]?.ToString()!);
        Settings.DownloadPrerelease = bool.Parse(settings["DownloadPrerelease"]?.ToString()!);
        Settings.AskEnableDependenciesWhenInstalling = Enum.Parse<AskType>(settings["AskEnableDependenciesWhenInstalling"]?.ToString()!);
        Settings.AskEnableDependenciesWhenEnabling = Enum.Parse<AskType>(settings["AskEnableDependenciesWhenEnabling"]?.ToString()!);
        Settings.AskDisableDependenciesWhenDeleting = Enum.Parse<AskType>(settings["AskDisableDependenciesWhenDeleting"]?.ToString()!);
        Settings.AskDisableDependenciesWhenDisabling = Enum.Parse<AskType>(settings["AskDisableDependenciesWhenDisabling"]?.ToString()!);
        _logger.Information("Saved setting loaded from Settings.json");
    }

    /// <summary>
    ///     If Updater files exist, delete them
    /// </summary>
    private void DeleteUpdater()
    {
        var updateDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Update");
        var updaterPath = Path.Combine(updateDirectory, "Updater.exe");
        if (_fileSystem.File.Exists(updaterPath))
        {
            _fileSystem.File.Delete(updaterPath);
            _logger.Information("Updater.exe found, deleting it");
        }

        if (_fileSystem.Directory.Exists(updateDirectory))
        {
            _fileSystem.Directory.Delete(updateDirectory);
            _logger.Information("Update directory found, deleting it");
        }
    }
}