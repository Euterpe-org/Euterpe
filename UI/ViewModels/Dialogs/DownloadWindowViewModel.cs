using System.IO;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using DialogHostAvalonia;
using ICSharpCode.SharpZipLib.Zip;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Extensions;
using static MuseDashModToolsUI.Localization.Resources;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.ViewModels.Dialogs;

public partial class DownloadWindowViewModel : ViewModelBase, IDownloadWindowViewModel
{
    [ObservableProperty] private string _downloadProgress = "Download progress: 0%";
    [ObservableProperty] private double _percentage;
    public IMessageBoxService MessageBoxService { get; init; }
    public IGitHubService GitHubService { get; init; }
    public ILogger Logger { get; init; }
    public ISavingService SavingService { get; init; }

    public async Task InstallMelonLoader()
    {
        if (File.Exists(SavingService.Settings.MelonLoaderZipPath))
        {
            var onlineSize = await GitHubService.GetMelonLoaderFileSize();
            var zipInfo = new FileInfo(SavingService.Settings.MelonLoaderZipPath);
            if (onlineSize > zipInfo.Length && !await DownloadMelonLoaderZipFile()) return;
        }
        else if (!await DownloadMelonLoaderZipFile())
        {
            return;
        }

        if (!await ExtractMelonLoaderZipFile()) return;
        if (!await DeleteMelonLoaderZipFile()) return;

        Logger.Information("MelonLoader install success");
        await MessageBoxService.CreateSuccessMessageBox(MsgBox_Content_InstallMelonLoaderSuccess.Localize());
        DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
    }

    private async Task<bool> DownloadMelonLoaderZipFile()
    {
        try
        {
            var downloadProgress = new Progress<double>(UpdateDownloadProgress);
            Logger.Information("Start downloading MelonLoader.zip");
            await GitHubService.DownloadMelonLoader(SavingService.Settings.MelonLoaderZipPath, downloadProgress);
            return true;
        }
        catch (Exception ex)
        {
            if (ex is HttpRequestException)
            {
                Logger.Error(ex, "Download MelonLoader.zip failed");
                await MessageBoxService.CreateErrorMessageBox(string.Format(MsgBox_Content_InstallMelonLoaderFailed_Internet.Localize(),
                    ex));
                DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
                return false;
            }

            Logger.Error(ex, "Download MelonLoader.zip failed");
            await MessageBoxService.CreateErrorMessageBox(string.Format(MsgBox_Content_InstallMelonLoaderFailed.Localize(), ex));
            DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
            return false;
        }
    }

    private async Task<bool> ExtractMelonLoaderZipFile()
    {
        try
        {
            Logger.Information("Extracting MelonLoader.zip");
            var fastZip = new FastZip();
            fastZip.ExtractZip(SavingService.Settings.MelonLoaderZipPath, SavingService.Settings.MuseDashFolder, FastZip.Overwrite.Always,
                null, null, null, true);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Extracting MelonLoader.zip failed");
            await MessageBoxService.CreateErrorMessageBox(string.Format(MsgBox_Content_UnzipMelonLoaderFailed.Localize(),
                SavingService.Settings.MelonLoaderZipPath, ex));
            DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
            return false;
        }
    }

    private async Task<bool> DeleteMelonLoaderZipFile()
    {
        try
        {
            Logger.Information("Deleting MelonLoader.zip");
            File.Delete(SavingService.Settings.MelonLoaderZipPath);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Deleting MelonLoader.zip failed");
            await MessageBoxService.CreateErrorMessageBox(string.Format(MsgBox_Content_DeleteMelonLoaderZipFailed.Localize(),
                SavingService.Settings.MelonLoaderZipPath, ex));
            DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
            return false;
        }
    }

    private void UpdateDownloadProgress(double value)
    {
        Percentage = value;
        DownloadProgress = "Download progress: " + value + "%";
    }
}