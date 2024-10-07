namespace MuseDashModTools.Abstractions;

public interface IFileSystemService
{
    bool CheckFileExists(string filePath);
    bool TryDeleteFile(string filePath, DeleteOption deleteOption = DeleteOption.FailIfNotFound);
    bool CheckDirectoryExists(string directoryPath);
    bool TryDeleteDirectory(string directoryPath, DeleteOption deleteOption = DeleteOption.FailIfNotFound);
}