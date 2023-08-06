using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;
using static MuseDashModToolsUI.Localization.Resources;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public class SavingService : ISavingService
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    private static string ConfigFolderPath
    {
        get
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MuseDashModTools");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }
    }

    private static string SettingPath => Path.Combine(ConfigFolderPath, "Settings.json");
    public IMessageBoxService MessageBoxService { get; init; }
    public Lazy<ILogAnalysisViewModel> LogAnalysisViewModel { get; init; }
    public Lazy<IModManageViewModel> ModManageViewModel { get; init; }
    public Lazy<ISettingsViewModel> SettingsViewModel { get; init; }

    public SavingService(ILogger logger, IFileSystem fileSystem)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        Load().Wait();
    }

    public Setting Settings { get; private set; } = new();

    public async Task InitializeSettings()
    {
        _logger.Information("Initializing settings...");
        try
        {
            if (!_fileSystem.File.Exists(SettingPath))
            {
                _logger.Error("Settings.json not found, creating new one");
                await MessageBoxService.CreateErrorMessageBox("Warning", MsgBox_Content_ChoosePath.Localize());
                await OnChoosePath();
                return;
            }

            var text = await _fileSystem.File.ReadAllTextAsync(SettingPath);
            var settings = JsonConvert.DeserializeObject<Setting>(text)!;
            if (string.IsNullOrEmpty(settings.MuseDashFolder))
            {
                _logger.Error("Settings.json stored path is empty, asking user to choose path");
                await MessageBoxService.CreateWarningMessageBox(MsgBox_Content_NullPath.Localize());
                await OnChoosePath();
                await InitializeSettings();
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

            Settings = settings.Clone();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred while initializing settings");
            await MessageBoxService.CreateErrorMessageBox(ex.ToString());
        }
    }

    public async Task Save()
    {
        var json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
        await _fileSystem.File.WriteAllTextAsync(SettingPath, json);
        _logger.Information("Settings saved");
    }

    public async Task OnChoosePath()
    {
        while (true)
        {
            if (Application.Current!.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime) continue;
            _logger.Information("Showing choose folder dialogue");

            var dialogue = await new Window().StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                { AllowMultiple = false, Title = FolderDialog_Title });
            if (dialogue.Count == 0)
            {
                if (!string.IsNullOrEmpty(Settings.MuseDashFolder))
                {
                    _logger.Information("Path not changed");
                    return;
                }

                _logger.Error("Invalid path, showing error message box");
                await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_InvalidPath);
                continue;
            }

            var path = dialogue[0].TryGetLocalPath();
            if (path == Settings.MuseDashFolder)
            {
                _logger.Information("Path not changed");
                return;
            }

            _logger.Information("User chose path {Path}", path);
            Settings.MuseDashFolder = path;
            Settings.LanguageCode ??= CultureInfo.CurrentUICulture.Name;

            var json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
            await _fileSystem.File.WriteAllTextAsync(SettingPath, json);
            _logger.Information("Settings saved to Settings.json");

            SettingsViewModel.Value.Initialize();
            ModManageViewModel.Value.Initialize();
            LogAnalysisViewModel.Value.Initialize();
            return;
        }
    }

    private async Task Load()
    {
        await LoadSavedSetting();
        DeleteUpdater();
    }

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