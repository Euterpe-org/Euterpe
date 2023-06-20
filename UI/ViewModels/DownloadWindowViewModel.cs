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
using Serilog;
using static MuseDashModToolsUI.Localization.Resources;

namespace MuseDashModToolsUI.ViewModels;

public partial class DownloadWindowViewModel : ViewModelBase, IDownloadWindowViewModel
{
    private readonly IDialogueService _dialogueService;
    private readonly IGitHubService _gitHubService;
    private readonly ILogger _logger;
    private readonly ISettingService _settingService;
    [ObservableProperty] private string _downloadProgress = "Download progress: 0%";
    [ObservableProperty] private double _percentage;

    public DownloadWindowViewModel(IDialogueService dialogueService, IGitHubService gitHubService, ILogger logger,
        ISettingService settingService)
    {
        _settingService = settingService;
        _gitHubService = gitHubService;
        _logger = logger;
        _dialogueService = dialogueService;
    }

    public async Task InstallMelonLoader()
    {
        var zipPath = Path.Join(_settingService.Settings.MuseDashFolder, "MelonLoader.zip");
        var downloadProgress = new Progress<double>(UpdateDownloadProgress);
        if (!File.Exists(zipPath))
            try
            {
                _logger.Information("Start downloading MelonLoader.zip");
                await _gitHubService.DownloadMelonLoader(zipPath, downloadProgress);
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException)
                {
                    _logger.Error("Download MelonLoader.zip failed: {Exception}", ex.ToString());
                    await _dialogueService.CreateErrorMessageBox(string.Format(MsgBox_Content_InstallMelonLoaderFailed_Internet.Localize(),
                        ex));
                    DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
                    return;
                }

                _logger.Error("Download MelonLoader.zip failed: {Exception}", ex.ToString());
                await _dialogueService.CreateErrorMessageBox(string.Format(MsgBox_Content_InstallMelonLoaderFailed.Localize(), ex));
                DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
                return;
            }

        try
        {
            _logger.Information("Extracting MelonLoader.zip");
            var fastZip = new FastZip();
            fastZip.ExtractZip(zipPath, _settingService.Settings.MuseDashFolder, FastZip.Overwrite.Always, null, null, null, true);
        }
        catch (Exception ex)
        {
            _logger.Error("Extracting MelonLoader.zip failed: {Exception}", ex.ToString());
            await _dialogueService.CreateErrorMessageBox(string.Format(MsgBox_Content_UnzipMelonLoaderFailed.Localize(), zipPath, ex));
            DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
            return;
        }

        try
        {
            _logger.Information("Deleting MelonLoader.zip");
            File.Delete(zipPath);
        }
        catch (Exception ex)
        {
            _logger.Error("Deleting MelonLoader.zip failed: {Exception}", ex.ToString());
            await _dialogueService.CreateErrorMessageBox(string.Format(MsgBox_Content_DeleteMelonLoaderZipFailed.Localize(), zipPath, ex));
            DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
            return;
        }

        _logger.Information("MelonLoader install success");
        await _dialogueService.CreateMessageBox(MsgBox_Title_Success, MsgBox_Content_InstallMelonLoaderSuccess.Localize());
        DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
    }

    private void UpdateDownloadProgress(double value)
    {
        Percentage = value;
        DownloadProgress = "Download progress: " + value + "%";
    }
}