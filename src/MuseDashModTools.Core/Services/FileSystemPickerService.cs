using Avalonia.Platform.Storage;

namespace MuseDashModTools.Core;

internal sealed class FileSystemPickerService : IFileSystemPickerService
{
    [UsedImplicitly]
    public TopLevelProxy TopLevel { get; init; } = null!;

    public async Task<string?> GetSingleFolderPathAsync(string dialogTitle)
    {
        var dialog = await TopLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                Title = dialogTitle
            }).ConfigureAwait(true);

        return dialog is not [] ? dialog[0].TryGetLocalPath() : null;
    }

    public async Task<IEnumerable<string?>> GetMultipleFolderPathAsync(string dialogTitle)
    {
        var dialog = await TopLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                AllowMultiple = true,
                Title = dialogTitle
            }).ConfigureAwait(true);

        return dialog.Select(x => x.TryGetLocalPath());
    }

    public async Task<string?> GetSingleFilePathAsync(string dialogTitle)
    {
        var dialog = await TopLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                AllowMultiple = false,
                Title = dialogTitle
            }).ConfigureAwait(true);

        return dialog is not [] ? dialog[0].TryGetLocalPath() : null;
    }

    public async Task<IEnumerable<string?>> GetMultipleFilePathAsync(string dialogTitle)
    {
        var dialog = await TopLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                AllowMultiple = true,
                Title = dialogTitle
            }).ConfigureAwait(true);

        return dialog.Select(x => x.TryGetLocalPath());
    }
}