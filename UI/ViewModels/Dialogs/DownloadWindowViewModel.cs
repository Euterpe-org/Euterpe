using System.IO;
using DialogHostAvalonia;
using ICSharpCode.SharpZipLib.Zip;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.ViewModels.Dialogs;

public partial class DownloadWindowViewModel : ViewModelBase, IDownloadWindowViewModel
{
    [ObservableProperty] private string _downloadProgress = "Download progress: 0%";
    [ObservableProperty] private double _percentage;

    [UsedImplicitly]
    public IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public IGitHubService GitHubService { get; init; }

    [UsedImplicitly]
    public ILogger Logger { get; init; }

    [UsedImplicitly]
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
        await MessageBoxService.SuccessMessageBox(MsgBox_Content_InstallMelonLoaderSuccess);
        DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
    }

    private async Task<bool> DownloadMelonLoaderZipFile()
    {
        var downloadProgress = new Progress<double>(UpdateDownloadProgress);
        Logger.Information("Start downloading MelonLoader");
        return await GitHubService.DownloadMelonLoader(downloadProgress);
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
            await MessageBoxService.ErrorMessageBox(string.Format(MsgBox_Content_UnzipMelonLoaderFailed,
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
            await MessageBoxService.ErrorMessageBox(string.Format(MsgBox_Content_DeleteMelonLoaderZipFailed,
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