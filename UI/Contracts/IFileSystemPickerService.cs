namespace MuseDashModToolsUI.Contracts;

public interface IFileSystemPickerService
{
    /// <summary>
    ///     Get single folder path with title
    /// </summary>
    /// <param name="title"></param>
    /// <returns>Folder path</returns>
    Task<string?> GetSingleFolderPath(string title);
}