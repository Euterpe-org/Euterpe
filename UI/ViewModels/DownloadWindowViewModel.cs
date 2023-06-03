using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DialogHostAvalonia;
using ICSharpCode.SharpZipLib.Zip;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Extensions;
using static MuseDashModToolsUI.Localization.Resources;

namespace MuseDashModToolsUI.ViewModels;

public partial class DownloadWindowViewModel : ViewModelBase, IDownloadWindowViewModel
{
    private readonly IDialogueService _dialogueService;
    private readonly IGitHubService _gitHubService;
    private readonly ISettingService _settings;
    [ObservableProperty] private string _downloadProgress = "Download progress: 0%";
    [ObservableProperty] private double _percentage;

    public DownloadWindowViewModel(IDialogueService dialogueService, IGitHubService gitHubService, ISettingService settings)
    {
        _settings = settings;
        _gitHubService = gitHubService;
        _dialogueService = dialogueService;
    }

    public async Task InstallMelonLoader()
    {
        var zipPath = Path.Join(_settings.Settings.MuseDashFolder, "MelonLoader.zip");
        var downloadProgress = new Progress<double>(UpdateDownloadProgress);
        if (!File.Exists(zipPath))
            try
            {
                await _gitHubService.DownloadMelonLoader(zipPath, downloadProgress);
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException)
                {
                    await _dialogueService.CreateErrorMessageBox(string.Format(MsgBox_Content_InstallMelonLoaderFailed_Internet.Localize(),
                        ex));
                    DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
                    return;
                }

                await _dialogueService.CreateErrorMessageBox(string.Format(MsgBox_Content_InstallMelonLoaderFailed.Localize(), ex));
                DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
                return;
            }

        try
        {
            var fastZip = new FastZip();
            fastZip.ExtractZip(zipPath, _settings.Settings.MuseDashFolder, FastZip.Overwrite.Always, null, null, null, true);
        }
        catch (Exception ex)
        {
            await _dialogueService.CreateErrorMessageBox(string.Format(MsgBox_Content_UnzipMelonLoaderFailed.Localize(), zipPath, ex));
            DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
            return;
        }

        try
        {
            File.Delete(zipPath);
        }
        catch (Exception ex)
        {
            await _dialogueService.CreateErrorMessageBox(string.Format(MsgBox_Content_DeleteMelonLoaderZipFailed.Localize(), zipPath, ex));
            DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
            return;
        }

        await _dialogueService.CreateMessageBox(MsgBox_Title_Success, MsgBox_Content_InstallMelonLoaderSuccess.Localize());
        DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
    }

    private void UpdateDownloadProgress(double value)
    {
        Percentage = value;
        DownloadProgress = "Download progress: " + value + "%";
    }
}