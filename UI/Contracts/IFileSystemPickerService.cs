namespace MuseDashModToolsUI.Contracts;

public interface IFileSystemPickerService
{
    Task<string?> GetSingleFolderPathAsync(string dialogTitle);
}