using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Models;
using Serilog;
using static MuseDashModToolsUI.Localization.Resources;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public class SettingService : ISettingService
{
    private readonly ILogger _logger;
    public IMessageBoxService MessageBoxService { get; init; }
    public Lazy<ILogAnalysisViewModel> LogAnalysisViewModel { get; init; }
    public Lazy<IModManageViewModel> ModManageViewModel { get; init; }
    public Lazy<ISettingsViewModel> SettingsViewModel { get; init; }

    public SettingService(ILogger logger)
    {
        _logger = logger;
        LoadSavedSetting().Wait();
    }

    public Setting Settings { get; private set; } = new();

    public async Task InitializeSettings()
    {
        _logger.Information("Initializing settings...");
        try
        {
            if (!File.Exists("Settings.json"))
            {
                _logger.Error("Settings.json not found, creating new one");
                await MessageBoxService.CreateErrorMessageBox("Warning", MsgBox_Content_ChoosePath.Localize());
                await OnChoosePath();
                return;
            }

            var text = await File.ReadAllTextAsync("Settings.json");
            var settings = JsonSerializer.Deserialize<Setting>(text)!;
            if (string.IsNullOrEmpty(settings.MuseDashFolder))
            {
                _logger.Error("Settings.json stored path is empty, asking user to choose path");
                await MessageBoxService.CreateErrorMessageBox(MsgBox_Title_Warning, MsgBox_Content_NullPath.Localize());
                await OnChoosePath();
                await InitializeSettings();
            }

            if (string.IsNullOrEmpty(settings.LanguageCode))
            {
                settings.LanguageCode = CultureInfo.CurrentUICulture.ToString();
                _logger.Warning("Settings.json stored language code is empty, using system language");
            }

            Settings = settings.Clone();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred while initializing settings");
            await MessageBoxService.CreateErrorMessageBox(ex.ToString());
        }
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

            var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync("Settings.json", json);
            _logger.Information("Settings saved to Settings.json");

            SettingsViewModel.Value.Initialize();
            ModManageViewModel.Value.Initialize();
            LogAnalysisViewModel.Value.Initialize();
            return;
        }
    }

    private async Task LoadSavedSetting()
    {
        if (!File.Exists("Settings.json"))
            return;
        var text = await File.ReadAllTextAsync("Settings.json");
        var settings = JsonNode.Parse(text);

        Settings.MuseDashFolder = settings?["MuseDashFolder"]?.ToString();
        Settings.LanguageCode = settings?["LanguageCode"]?.ToString();
        Settings.FontName = settings?["FontName"]?.ToString();
        Settings.DownloadSource = Enum.Parse<DownloadSources>(settings?["DownloadSource"]?.ToString()!);
        Settings.AskEnableDependenciesWhenInstalling = Enum.Parse<AskType>(settings?["AskEnableDependenciesWhenInstalling"]?.ToString()!);
        Settings.AskEnableDependenciesWhenEnabling = Enum.Parse<AskType>(settings?["AskEnableDependenciesWhenEnabling"]?.ToString()!);
        Settings.AskDisableDependenciesWhenDeleting = Enum.Parse<AskType>(settings?["AskDisableDependenciesWhenDeleting"]?.ToString()!);
        Settings.AskDisableDependenciesWhenDisabling = Enum.Parse<AskType>(settings?["AskDisableDependenciesWhenDisabling"]?.ToString()!);
        _logger.Information("Saved setting loaded from Settings.json");

        var updateDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Update");
        var updaterPath = Path.Combine(updateDirectory, "Updater.exe");
        if (File.Exists(updaterPath))
        {
            File.Delete(updaterPath);
            _logger.Information("Updater.exe found, deleting it");
        }

        if (Directory.Exists(updateDirectory))
        {
            Directory.Delete(updateDirectory);
            _logger.Information("Update directory found, deleting it");
        }
    }
}