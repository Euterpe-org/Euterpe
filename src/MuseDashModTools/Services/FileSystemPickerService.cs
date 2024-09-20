using Avalonia.Platform.Storage;

namespace MuseDashModTools.Services;

public sealed class FileSystemPickerService : IFileSystemPickerService
{
    public async Task<string?> GetSingleFolderPathAsync(string dialogTitle)
    {
        var dialogue = await GetCurrentMainWindow().StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                Title = dialogTitle
            }).ConfigureAwait(true);

        return dialogue is not [] ? dialogue[0].TryGetLocalPath() : null;
    }
}