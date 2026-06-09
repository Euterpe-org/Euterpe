using Avalonia.Platform.Storage;

namespace Euterpe.Abstractions;

public interface IFileSystemPickerService
{
    Task<IStorageFolder?> GetSingleFolderAsync(string dialogTitle);
    Task<string?> GetSingleFolderPathAsync(string dialogTitle);
    Task<IReadOnlyList<IStorageFolder>?> GetMultipleFoldersAsync(string dialogTitle);
    Task<IEnumerable<string?>?> GetMultipleFoldersPathAsync(string dialogTitle);
    Task<IStorageFile?> GetSingleFileAsync(string dialogTitle);
    Task<string?> GetSingleFilePathAsync(string dialogTitle);
    Task<IReadOnlyList<IStorageFile>?> GetMultipleFilesAsync(string dialogTitle, IReadOnlyList<FilePickerFileType>? extraFileTypes = null);
    Task<IEnumerable<string?>?> GetMultipleFilePathsAsync(string dialogTitle, IReadOnlyList<FilePickerFileType>? extraFileTypes = null);
}
