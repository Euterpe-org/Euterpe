namespace MuseDashModTools.Abstractions;

public interface IFileSystemPickerService
{
    Task<string?> GetSingleFolderPathAsync(string dialogTitle);
}