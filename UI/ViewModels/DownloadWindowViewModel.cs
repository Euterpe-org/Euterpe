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
    [ObservableProperty] private string _downloadProgress = "Download progress: 0%";
    [ObservableProperty] private double _percentage;
    public IDialogueService? DialogueService { get; init; }
    public IGitHubService? GitHubService { get; init; }
    public ILogger? Logger { get; init; }
    public ISettingService? SettingService { get; init; }

    public async Task InstallMelonLoader()
    {
        var zipPath = Path.Join(SettingService?.Settings.MuseDashFolder, "MelonLoader.zip");
        var downloadProgress = new Progress<double>(UpdateDownloadProgress);
        if (!File.Exists(zipPath))
            try
            {
                Logger?.Information("Start downloading MelonLoader.zip");
                await GitHubService!.DownloadMelonLoader(zipPath, downloadProgress);
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException)
                {
                    Logger?.Error(ex, "Download MelonLoader.zip failed");
                    await DialogueService!.CreateErrorMessageBox(string.Format(MsgBox_Content_InstallMelonLoaderFailed_Internet.Localize(),
                        ex));
                    DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
                    return;
                }

                Logger?.Error(ex, "Download MelonLoader.zip failed");
                await DialogueService!.CreateErrorMessageBox(string.Format(MsgBox_Content_InstallMelonLoaderFailed.Localize(), ex));
                DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
                return;
            }


        try
        {
            Logger?.Information("Extracting MelonLoader.zip");
            var fastZip = new FastZip();
            fastZip.ExtractZip(zipPath, SettingService?.Settings.MuseDashFolder, FastZip.Overwrite.Always, null, null, null, true);
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Extracting MelonLoader.zip failed");
            await DialogueService!.CreateErrorMessageBox(string.Format(MsgBox_Content_UnzipMelonLoaderFailed.Localize(), zipPath, ex));
            DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
            return;
        }

        try
        {
            Logger?.Information("Deleting MelonLoader.zip");
            File.Delete(zipPath);
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Deleting MelonLoader.zip failed");
            await DialogueService!.CreateErrorMessageBox(string.Format(MsgBox_Content_DeleteMelonLoaderZipFailed.Localize(), zipPath, ex));
            DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
            return;
        }

        Logger?.Information("MelonLoader install success");
        await DialogueService!.CreateMessageBox(MsgBox_Title_Success, MsgBox_Content_InstallMelonLoaderSuccess.Localize());
        DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
    }

    private void UpdateDownloadProgress(double value)
    {
        Percentage = value;
        DownloadProgress = "Download progress: " + value + "%";
    }
}