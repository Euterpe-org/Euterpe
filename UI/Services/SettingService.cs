using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Models;
using static MuseDashModToolsUI.Localization.Resources;

namespace MuseDashModToolsUI.Services;

public class SettingService : ISettingService
{
    private readonly IDialogueService _dialogueService;

    public SettingService(IDialogueService dialogueService)
    {
        _dialogueService = dialogueService;
    }

    public Setting Settings { get; set; } = new();

    public async Task InitializeSettings()
    {
        try
        {
            if (!File.Exists("Settings.json"))
            {
                await _dialogueService.CreateErrorMessageBox("Warning", MsgBox_Content_ChoosePath.Localize());
                await OnChoosePath();
                return;
            }

            var text = await File.ReadAllTextAsync("Settings.json");
            var settings = JsonSerializer.Deserialize<Setting>(text)!;
            if (string.IsNullOrEmpty(settings.MuseDashFolder))
            {
                await _dialogueService.CreateErrorMessageBox(MsgBox_Title_Warning, MsgBox_Content_NullPath.Localize());
                await OnChoosePath();
                await InitializeSettings();
            }

            if (string.IsNullOrEmpty(settings.Language)) settings.Language = CultureInfo.CurrentUICulture.ToString();

            Settings = settings.Clone();

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
            var dialogue = new OpenFolderDialog { Title = FolderDialog_Title };
            if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var path = await dialogue.ShowAsync(desktop.MainWindow);
                if (string.IsNullOrEmpty(path))
                {
                    await _dialogueService.CreateErrorMessageBox(MsgBox_Content_InvalidPath);
                    continue;
                }

                Settings.MuseDashFolder = path;
                Settings.Language = CultureInfo.CurrentUICulture.ToString();
                var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync("Settings.json", json);
            }

            break;
        }
    }
}