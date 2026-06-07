using Avalonia.Platform.Storage;

namespace Euterpe.Core;

internal sealed class FileSystemPickerService : IFileSystemPickerService
{
    public required TopLevelProxy TopLevel { get; init; }

    public async Task<IStorageFolder?> GetSingleFolderAsync(string dialogTitle)
    {
        var folders = await TopLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                Title = dialogTitle
            }).ConfigureAwait(true);

        return folders is not [] ? folders[0] : null;
    }

    public async Task<string?> GetSingleFolderPathAsync(string dialogTitle)
    {
        var folders = await TopLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                Title = dialogTitle
            }).ConfigureAwait(true);

        return folders is not [] ? folders[0].TryGetLocalPath() : null;
    }

    public async Task<IReadOnlyList<IStorageFolder>?> GetMultipleFoldersAsync(string dialogTitle)
    {
        var folders = await TopLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                AllowMultiple = true,
                Title = dialogTitle
            }).ConfigureAwait(true);

        return folders is not [] ? folders : null;
    }

    public async Task<IEnumerable<string?>?> GetMultipleFoldersPathAsync(string dialogTitle)
    {
        var folders = await TopLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                AllowMultiple = true,
                Title = dialogTitle
            }).ConfigureAwait(true);

        return folders is not [] ? folders.Select(x => x.TryGetLocalPath()) : null;
    }

    public async Task<IStorageFile?> GetSingleFileAsync(string dialogTitle)
    {
        var files = await TopLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                AllowMultiple = false,
                Title = dialogTitle
            }).ConfigureAwait(true);

        return files is not [] ? files[0] : null;
    }

    public async Task<string?> GetSingleFilePathAsync(string dialogTitle)
    {
        var files = await TopLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                AllowMultiple = false,
                Title = dialogTitle
            }).ConfigureAwait(true);

        return files is not [] ? files[0].TryGetLocalPath() : null;
    }

    public async Task<IReadOnlyList<IStorageFile>?> GetMultipleFilesAsync(string dialogTitle, IReadOnlyList<FilePickerFileType>? extraFileTypes = null)
    {
        var files = await TopLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                AllowMultiple = true,
                Title = dialogTitle,
                FileTypeFilter =
                [
                    .. extraFileTypes ?? [],
                    new FilePickerFileType(FilePickerFileType_AllFiles) { Patterns = ["*.*"] }
                ]
            }).ConfigureAwait(true);

        return files is not [] ? files : null;
    }

    public async Task<IEnumerable<string?>?> GetMultipleFilePathsAsync(string dialogTitle, IReadOnlyList<FilePickerFileType>? extraFileTypes = null)
    {
        var files = await TopLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                AllowMultiple = true,
                Title = dialogTitle,
                FileTypeFilter =
                [
                    .. extraFileTypes ?? [],
                    new FilePickerFileType(FilePickerFileType_AllFiles) { Patterns = ["*.*"] }
                ]
            }).ConfigureAwait(true);

        return files is not [] ? files.Select(x => x.TryGetLocalPath()) : null;
    }
}