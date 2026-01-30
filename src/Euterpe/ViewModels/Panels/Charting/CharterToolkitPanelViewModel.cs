using Avalonia.Platform.Storage;

namespace Euterpe.ViewModels.Panels.Charting;

public sealed partial class CharterToolkitPanelViewModel : ViewModelBase
{
    private static readonly FilePickerFileType MdmFileType = new("MDM Files") { Patterns = ["*.mdm"] };

    [RelayCommand]
    private async Task CreateMdmFilesAsync()
    {
        var folders = await FileSystemPickerService.GetMultipleFoldersAsync(
                FolderDialog_Title_ChooseChartFolder)
            .ConfigureAwait(true);

        if (folders is null)
        {
            return;
        }

        foreach (var folder in folders)
        {
            var fileName = $"{folder.Name}.mdm";

            try
            {
                var folderPath = folder.TryGetLocalPath();
                if (folderPath.IsNullOrEmpty())
                {
                    continue;
                }

                var parentPath = Path.GetDirectoryName(folderPath);
                var targetPath = parentPath.IsNullOrEmpty() ? fileName : Path.Combine(parentPath, fileName);

                await ArchiveService.CreateZipFileAsync(folderPath, targetPath).ConfigureAwait(false);
                Logger.ZLogInformation($"Created MDM file {fileName} from folder: {folder.Name}");
            }
            catch (Exception ex)
            {
                Logger.ZLogError(ex, $"Failed to create MDM file {fileName} from folder: {folder.Name}");
                await MessageBoxService.ErrorOverlayAsync(MessageBox_Content_MdmFile_Create_Failed).ConfigureAwait(false);
            }
        }

        await MessageBoxService.SuccessOverlayAsync(MessageBox_Content_MdmFile_Create_Success).ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task ExtractMdmFilesAsync()
    {
        var files = await FileSystemPickerService.GetMultipleFilesAsync(
                FileDialog_Title_ChooseChartFile, [MdmFileType])
            .ConfigureAwait(true);

        if (files is null)
        {
            return;
        }

        foreach (var file in files)
        {
            var folderName = Path.GetFileNameWithoutExtension(file.Name);

            try
            {
                var mdmPath = file.TryGetLocalPath();
                if (mdmPath.IsNullOrEmpty())
                {
                    continue;
                }

                var parentPath = Path.GetDirectoryName(mdmPath);
                var targetPath = parentPath.IsNullOrEmpty() ? folderName : Path.Combine(parentPath, folderName);

                await ArchiveService.ExtractZipFileAsync(mdmPath, targetPath).ConfigureAwait(false);
                Logger.ZLogInformation($"Extracted MDM file {file.Name} to folder: {folderName}");
            }
            catch (Exception ex)
            {
                Logger.ZLogError(ex, $"Failed to extract MDM file {file.Name} to folder: {folderName}");
                await MessageBoxService.ErrorOverlayAsync(MessageBox_Content_MdmFile_Extract_Failed).ConfigureAwait(false);
            }
        }

        await MessageBoxService.SuccessOverlayAsync(MessageBox_Content_MdmFile_Extract_Success).ConfigureAwait(false);
    }

    #region Injections

    [UsedImplicitly]
    public required IArchiveService ArchiveService { get; init; }

    [UsedImplicitly]
    public required ILogger<CharterToolkitPanelViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required IFileSystemPickerService FileSystemPickerService { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}