using Avalonia.Platform.Storage;

namespace MuseDashModToolsUI.Services;

public sealed class FileSystemPickerService : IFileSystemPickerService
{
    public async Task<string?> GetSingleFolderPath(string dialogTitle)
    {
        var dialogue = await GetCurrentMainWindow().StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            { AllowMultiple = false, Title = dialogTitle });

        return dialogue is not [] ? dialogue[0].TryGetLocalPath() : null;
    }
}