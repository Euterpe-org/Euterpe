using Avalonia.Platform.Storage;

namespace MuseDashModTools.Services;

public sealed class FileSystemPickerService : IFileSystemPickerService
{
    [UsedImplicitly]
    public TopLevel TopLevel { get; init; } = null!;

    public async Task<string?> GetSingleFolderPathAsync(string dialogTitle)
    {
        var dialogue = await TopLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                Title = dialogTitle
            }).ConfigureAwait(true);

        return dialogue is not [] ? dialogue[0].TryGetLocalPath() : null;
    }
}