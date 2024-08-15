using Avalonia.Platform.Storage;

namespace MuseDashModToolsUI.Services;

public sealed class FileSystemPickerService : IFileSystemPickerService
{
    public async Task<string?> GetSingleFolderPath(string title)
    {
        var dialogue = await new Window().StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            { AllowMultiple = false, Title = title });

        return dialogue is not [] ? dialogue[0].TryGetLocalPath() : null;
    }
}