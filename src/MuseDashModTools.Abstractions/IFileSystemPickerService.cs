namespace MuseDashModTools.Abstractions;

public interface IFileSystemPickerService
{
    Task<string?> GetSingleFolderPathAsync(string dialogTitle);
    Task<IEnumerable<string?>> GetMultipleFolderPathAsync(string dialogTitle);
    Task<string?> GetSingleFilePathAsync(string dialogTitle);
    Task<IEnumerable<string?>> GetMultipleFilePathAsync(string dialogTitle);
}