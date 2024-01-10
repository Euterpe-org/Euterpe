using Avalonia.Platform.Storage;

namespace MuseDashModToolsUI.Services;

public sealed class FileSystemPickerService : IFileSystemPickerService
{
    public async Task<string?> GetSingleFolderPathAsync(string title)
    {
        var dialogue = await new Window().StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            { AllowMultiple = false, Title = title });

        return dialogue.Count != 0 ? dialogue[0].TryGetLocalPath() : null;
    }
}