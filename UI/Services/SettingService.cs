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

namespace MuseDashModToolsUI.Services;

public class SettingService : ISettingService
{
    public ILogger? Logger { get; init; }
    public Lazy<ISettingsViewModel>? SettingsViewModel { get; init; }
    public IDialogueService? DialogueService { get; init; }
    public SettingService() => Task.Run(InitializeLanguageAndPath);
    public Setting Settings { get; private set; } = new();

    public async Task InitializeSettings()
    {
        Logger?.Information("Initializing settings...");
        try
        {
            if (!File.Exists("Settings.json"))
            {
                Logger?.Error("Settings.json not found, creating new one");
                await DialogueService!.CreateErrorMessageBox("Warning", MsgBox_Content_ChoosePath.Localize());
                await OnChoosePath();
                return;
            }

            var text = await File.ReadAllTextAsync("Settings.json");
            var settings = JsonSerializer.Deserialize<Setting>(text)!;
            if (string.IsNullOrEmpty(settings.MuseDashFolder))
            {
                Logger?.Error("Settings.json stored path is empty, asking user to choose path");
                await DialogueService!.CreateErrorMessageBox(MsgBox_Title_Warning, MsgBox_Content_NullPath.Localize());
                await OnChoosePath();
                await InitializeSettings();
            }

            if (string.IsNullOrEmpty(settings.LanguageCode))
            {
                settings.LanguageCode = CultureInfo.CurrentUICulture.ToString();
                Logger?.Warning("Settings.json stored language code is empty, using system language");
            }

            Settings = settings.Clone();

            var updateDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Update");
            var updaterPath = Path.Combine(updateDirectory, "Updater.exe");
            if (File.Exists(updaterPath))
            {
                File.Delete(updaterPath);
                Logger?.Information("Updater.exe found, deleting it");
            }

            if (Directory.Exists(updateDirectory))
            {
                Directory.Delete(updateDirectory);
                Logger?.Information("Update directory found, deleting it");
            }
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Error occurred while initializing settings");
            await DialogueService!.CreateErrorMessageBox(ex.ToString());
        }
    }

    public async Task<bool> OnChoosePath()
    {
        while (true)
        {
            if (Application.Current!.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime) continue;
            Logger?.Information("Showing choose folder dialogue");

            var dialogue = await new Window().StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                { AllowMultiple = false, Title = FolderDialog_Title });
            if (dialogue.Count == 0)
            {
                if (!string.IsNullOrEmpty(Settings.MuseDashFolder))
                {
                    Logger?.Information("Path not changed");
                    return false;
                }

                Logger?.Error("Invalid path, showing error message box");
                await DialogueService!.CreateErrorMessageBox(MsgBox_Content_InvalidPath);
                continue;
            }

            var path = dialogue[0].TryGetLocalPath();
            if (path == Settings.MuseDashFolder)
            {
                Logger?.Information("Path not changed");
                return false;
            }

            Logger?.Information("User chose path {Path}", path);
            Settings.MuseDashFolder = path;
            Settings.LanguageCode = CultureInfo.CurrentUICulture.ToString();

            var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync("Settings.json", json);
            Logger?.Information("Settings saved to Settings.json");

            SettingsViewModel?.Value.Initialize();
            return true;
        }
    }

    private async Task InitializeLanguageAndPath()
    {
        if (!File.Exists("Settings.json"))
            return;
        var text = await File.ReadAllTextAsync("Settings.json");
        var setting = JsonNode.Parse(text);
        Settings.LanguageCode = setting?["LanguageCode"]?.ToString();
        Settings.MuseDashFolder = setting?["MuseDashFolder"]?.ToString();
        Logger?.Information("Language and Path loaded from Settings.json");
    }
}