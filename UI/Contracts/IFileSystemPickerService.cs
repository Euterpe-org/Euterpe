namespace MuseDashModToolsUI.Contracts;

public interface IFileSystemPickerService
{
    /// <summary>
    ///     Get single folder path with title
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    Task<string?> GetSingleFolderPath(string title);
}