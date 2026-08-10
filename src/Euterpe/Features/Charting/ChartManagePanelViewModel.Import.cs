using Avalonia.Platform.Storage;
using Euterpe.Models.Progress;

namespace Euterpe.Features.Charting;

public sealed partial class ChartManagePanelViewModel
{
    [RelayCommand]
    public Task MigrateCustomAlbumsAsync() =>
        RunWithProgressDialogAsync(ChartManage_Migrating, ChartManage_MigratingHint, false, async progress =>
        {
            var migratedCount = await ChartManageService.MigrateCustomAlbumsAsync(progress).ConfigureAwait(true);
            if (migratedCount is 0)
            {
                NotificationService.NoticeLight(Notification_Content_Migration_None);
            }
        });

    public Task DownloadChartAsync(string cid) =>
        RunWithProgressDialogAsync(ChartManage_Downloading, ChartManage_DownloadingHint, true, progress =>
            ChartManageService.DownloadChartAsync(cid, progress));

    [RelayCommand]
    private async Task ImportChartsAsync(IReadOnlyList<IStorageItem> files)
    {
        var paths = files.GetLocalPaths().OfType<string>().ToArray();
        if (paths is [])
        {
            return;
        }

        if (await ChartManageService.ImportChartsAsync(paths).ConfigureAwait(true))
        {
            Filter.Source = ChartSource.Offline;
        }
    }

    [RelayCommand]
    private Task ImportShareAsync() => ShareImportDialogService.ShowAsync();

    private async Task RunWithProgressDialogAsync(string title, string hint, bool indeterminate, Func<IProgress<BatchProgress>, Task> work)
    {
        ProgressDialogViewModel.Reset();
        ProgressDialogViewModel.IsIndeterminate = indeterminate;
        ProgressDialogViewModel.Hint = hint;

        var options = new OverlayDialogOptions
        {
            Title = title,
            CanDragMove = false,
            CanResize = false,
            IsCloseButtonVisible = false
        };

        var dialog = DialogService.ShowOverlayAsync<ProgressDialog, ProgressDialogViewModel>(
            ProgressDialogViewModel, options, MainWindowViewModel.DialogHostId);
        try
        {
            var progress = new Progress<BatchProgress>(ProgressDialogViewModel.Report);
            await work(progress).ConfigureAwait(true);
        }
        finally
        {
            ProgressDialogViewModel.Close();
            await dialog.ConfigureAwait(true);
        }
    }
}
