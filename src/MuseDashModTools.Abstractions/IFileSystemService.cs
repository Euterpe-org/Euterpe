namespace MuseDashModTools.Abstractions;

public interface IFileSystemService
{
    /// <summary>
    ///     Provides logging besides normal checking
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    bool CheckFileExists(string filePath);

    bool TryDeleteFile(string filePath, DeleteOption deleteOption = DeleteOption.FailIfNotFound);

    /// <summary>
    ///     Provides logging besides normal checking
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <returns></returns>
    bool CheckDirectoryExists(string directoryPath);

    bool TryDeleteDirectory(string directoryPath, DeleteOption deleteOption = DeleteOption.FailIfNotFound);
}