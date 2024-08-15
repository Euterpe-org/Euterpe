namespace MuseDashModToolsUI.Contracts;

public interface IFileSystemPickerService
{
    Task<string?> GetSingleFolderPath(string title);
}