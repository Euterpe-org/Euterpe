using System.IO.Compression;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.ViewModels.Dialogs;

public sealed partial class DownloadWindowViewModel : ViewModelBase, IDownloadWindowViewModel
{
    [ObservableProperty] private string _downloadProgress = "Download progress: 0%";
    [ObservableProperty] private double _percentage;

    [UsedImplicitly]
    public IDownloadService DownloadService { get; init; }

    [UsedImplicitly]
    public ILogger Logger { get; init; }

    [UsedImplicitly]
    public Setting Settings { get; init; }

    public async Task InstallMelonLoader()
    {
        if (File.Exists(Settings.MelonLoaderZipPath))
        {
            var onlineSize = await DownloadService.GetMelonLoaderFileSizeAsync();
            var zipInfo = new FileInfo(Settings.MelonLoaderZipPath);
            if (onlineSize > zipInfo.Length && !await DownloadMelonLoaderZipFile())
            {
                return;
            }
        }
        else if (!await DownloadMelonLoaderZipFile())
        {
            return;
        }

        if (!await ExtractMelonLoaderZipFile())
        {
            return;
        }

        if (!await DeleteMelonLoaderZipFile())
        {
            return;
        }

        Logger.Information("MelonLoader install success");
        await MessageBoxService.SuccessMessageBox(MsgBox_Content_InstallMelonLoaderSuccess);
        DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
    }

    private async Task<bool> DownloadMelonLoaderZipFile()
    {
        var downloadProgress = new Progress<double>(UpdateDownloadProgress);
        Logger.Information("Start downloading MelonLoader");
        return await DownloadService.DownloadMelonLoaderAsync(downloadProgress);
    }

    private async Task<bool> ExtractMelonLoaderZipFile()
    {
        try
        {
            Logger.Information("Extracting MelonLoader.zip");
            ZipFile.ExtractToDirectory(Settings.MelonLoaderZipPath, Settings.MuseDashFolder!, true);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Extracting MelonLoader.zip failed");
            await MessageBoxService.FormatErrorMessageBox(MsgBox_Content_UnzipMelonLoaderFailed, Settings.MelonLoaderZipPath, ex);
            DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
            return false;
        }
    }

    private async Task<bool> DeleteMelonLoaderZipFile()
    {
        try
        {
            Logger.Information("Deleting MelonLoader.zip");
            File.Delete(Settings.MelonLoaderZipPath);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Deleting MelonLoader.zip failed");
            await MessageBoxService.FormatErrorMessageBox(MsgBox_Content_DeleteMelonLoaderZipFailed,
                Settings.MelonLoaderZipPath, ex);
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