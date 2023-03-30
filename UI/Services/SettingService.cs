using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Services;

public class SettingService : ISettingService
{
    [JsonIgnore] private readonly IDialogueService _dialogueService;
    public Setting Settings { get; set; } = new();

    public SettingService(IDialogueService dialogueService)
    {
        _dialogueService = dialogueService;
    }

    public async Task InitializeSettings()
    {
        try
        {
            if (!File.Exists("Settings.json"))
            {
                await _dialogueService.CreateErrorMessageBox("Warning", "You haven't choose Muse Dash Folder\nPlease choose the folder");
                await OnChoosePath();
                return;
            }

            var text = await File.ReadAllTextAsync("Settings.json");
            var settings = JsonSerializer.Deserialize<Setting>(text)!;
            if (settings.MuseDashFolder is null)
            {
                await _dialogueService.CreateErrorMessageBox("Warning", "Your stored Muse Dash Folder path is null\nPlease choose the correct folder");
                await OnChoosePath();
                return;
            }

            Settings.MuseDashFolder = settings.MuseDashFolder;
            Settings.AskInstallMuseDashModTools = settings.AskInstallMuseDashModTools;
            Settings.AskEnableDependenciesWhenInstalling = settings.AskEnableDependenciesWhenInstalling;
            Settings.AskDisableDependenciesWhenDeleting = settings.AskDisableDependenciesWhenDeleting;
            Settings.AskEnableDependenciesWhenEnabling = settings.AskEnableDependenciesWhenEnabling;
            Settings.AskDisableDependenciesWhenDisabling = settings.AskDisableDependenciesWhenDisabling;

            var updateDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Update");
            var updaterPath = Path.Combine(updateDirectory, "Updater.exe");
            if (File.Exists(updaterPath))
                File.Delete(updaterPath);
            if (Directory.Exists(updateDirectory))
                Directory.Delete(updateDirectory);
        }
        catch (Exception ex)
        {
            await _dialogueService.CreateErrorMessageBox(ex.ToString());
        }
    }

    public async Task OnChoosePath()
    {
        while (true)
        {
            var dialogue = new OpenFolderDialog { Title = "Choose Muse Dash Folder" };
            if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var path = await dialogue.ShowAsync(desktop.MainWindow);
                if (string.IsNullOrEmpty(path))
                {
                    await _dialogueService.CreateErrorMessageBox("The path you chose is invalid. Try again...");
                    continue;
                }

                Settings.MuseDashFolder = path;
                var json = JsonSerializer.Serialize(this);
                await File.WriteAllTextAsync("Settings.json", json);
            }

            break;
        }
    }
}