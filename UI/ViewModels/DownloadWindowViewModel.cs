using System;
using System.IO;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using ICSharpCode.SharpZipLib.Zip;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.ViewModels;

public partial class DownloadWindowViewModel : ViewModelBase, IDownloadWindowViewModel
{
    [ObservableProperty] private double _percentage;
    [ObservableProperty] private bool _downloadFinished;
    [ObservableProperty] private string _downloadProgress = "Downloading progress: 0%";

    private readonly IGitHubService _gitHubService;
    private readonly IDialogueService _dialogueService;

    public DownloadWindowViewModel(IGitHubService gitHubService, IDialogueService dialogueService)
    {
        _gitHubService = gitHubService;
        _dialogueService = dialogueService;
    }

    public DownloadWindowViewModel(string gameFolderPath)
    {
        InstallMelonLoader(gameFolderPath);
    }

    private async void InstallMelonLoader(string gameFolderPath)
    {
        var zipPath = Path.Join(gameFolderPath, "MelonLoader.zip");
        if (!File.Exists(zipPath))
        {
            try
            {
                await _gitHubService.DownloadMelonLoader(zipPath, Percentage, DownloadFinished);
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
            fastZip.ExtractZip(zipPath, gameFolderPath, FastZip.Overwrite.Always, null, null, null, true);
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
    }

    partial void OnPercentageChanged(double value) => DownloadProgress = "Downloading progress: " + value + "%";
}