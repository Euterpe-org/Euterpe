namespace MuseDashModTools.Contracts;

public interface IFileSystemService
{
    bool CheckFileExists(string filePath);
    bool TryDeleteFile(string filePath);
    bool CheckDirectoryExists(string directoryPath);
    bool TryDeleteDirectory(string directoryPath);
}