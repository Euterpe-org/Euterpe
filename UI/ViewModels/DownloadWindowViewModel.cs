using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DialogHostAvalonia;
using ICSharpCode.SharpZipLib.Zip;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;

namespace MuseDashModToolsUI.ViewModels;

public partial class DownloadWindowViewModel : ViewModelBase, IDownloadWindowViewModel
{
    [ObservableProperty] private int _percentage;
    [ObservableProperty] private string _downloadProgress = "Download progress: 0%";

    private readonly IDialogueService _dialogueService;
    private readonly IGitHubService _gitHubService;
    private readonly ISettingService _settings;

    public DownloadWindowViewModel(IDialogueService dialogueService, IGitHubService gitHubService, ISettingService settings)
    {
        _settings = settings;
        _gitHubService = gitHubService;
        _dialogueService = dialogueService;
    }

    public async Task InstallMelonLoader()
    {
        var zipPath = Path.Join(_settings.Settings.MuseDashFolder, "MelonLoader.zip");
        var downloadProgress = new Progress<int>(UpdateDownloadProgress);
        if (!File.Exists(zipPath))
        {
            try
            {
                await _gitHubService.DownloadMelonLoader(zipPath, downloadProgress);
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException)
                {
                    await _dialogueService.CreateErrorMessageBox("MelonLoader download failed due to internet\nAre you online?");
                    return;
                }

                await _dialogueService.CreateErrorMessageBox($"MelonLoader download failed\n{ex}");
                return;
            }
        }

        try
        {
            var fastZip = new FastZip();
            fastZip.ExtractZip(zipPath, _settings.Settings.MuseDashFolder, FastZip.Overwrite.Always, null, null, null, true);
        }
        catch (Exception)
        {
            await _dialogueService.CreateErrorMessageBox($"Cannot unzip MelonLoader.zip in\n{zipPath}\nPlease make sure your game is not running\nThen try manually unzip");
            return;
        }

        try
        {
            File.Delete(zipPath);
        }
        catch (Exception)
        {
            await _dialogueService.CreateErrorMessageBox($"Failed to delete MelonLoader.zip in\n{zipPath}\nTry manually delete");
            return;
        }

        await _dialogueService.CreateMessageBox("Success", "MelonLoader has been successfully installed\n");
        DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
    }

    private void UpdateDownloadProgress(int value)
    {
        Percentage = value;
        DownloadProgress = "Download progress: " + value + "%";
    }
}