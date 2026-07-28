namespace Euterpe.Services;

public sealed class ShareImportDialogService
{
    public async Task ShowAsync(string? shareText = null)
    {
        ShareImportDialogViewModel.Prepare(shareText);

        var options = new OverlayDialogOptions
        {
            Title = XAML.Share_Import_Header,
            CanDragMove = false,
            CanResize = false,
            IsCloseButtonVisible = true
        };

        var canSwitch = GameSwitcher.CanSwitch;
        GameSwitcher.CanSwitch = false;
        try
        {
            await DialogService.ShowOverlayAsync<ShareImportDialog, ShareImportDialogViewModel>(
                ShareImportDialogViewModel, options, MainWindowViewModel.DialogHostId).ConfigureAwait(true);
        }
        finally
        {
            try
            {
                ShareImportDialogViewModel.CancelImport();
                if (ShareImportDialogViewModel.ImportCommand.ExecutionTask is { } importTask)
                {
                    await importTask.ConfigureAwait(true);
                }
            }
            finally
            {
                GameSwitcher.CanSwitch = canSwitch;
            }
        }
    }

    #region Injections

    public required IDialogService DialogService { get; init; }
    public required GameSwitcher GameSwitcher { get; init; }
    public required ShareImportDialogViewModel ShareImportDialogViewModel { get; init; }

    #endregion Injections
}
