namespace Euterpe.Core;

internal sealed class FileSystemService : IFileSystemService
{
    #region Injections

    public required ILogger<FileSystemService> Logger { get; init; }

    #endregion Injections

    public bool CheckFileExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            return true;
        }

        Logger.ZLogError($"{Path.GetFileName(filePath)} does not exists on {filePath}");
        return false;
    }

    public bool CheckDirectoryExists(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            return true;
        }

        Logger.ZLogError($"{Path.GetDirectoryName(directoryPath)} does not exists on {directoryPath}");
        return false;
    }

    public bool TryDeleteFile(string filePath, DeleteOption deleteOption = DeleteOption.FailIfNotFound)
    {
        if (deleteOption is DeleteOption.IgnoreIfNotFound && !File.Exists(filePath))
        {
            Logger.ZLogWarning($"{filePath} does not exists, skipping deletion");
            return true;
        }

        try
        {
            File.Delete(filePath);
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to delete file {filePath}");
            return false;
        }
    }

    public bool TryMoveFile(string sourcePath, string destinationPath)
    {
        try
        {
            File.Move(sourcePath, destinationPath);
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to move file from {sourcePath} to {destinationPath}");
            return false;
        }
    }

    public bool TryDeleteDirectory(string directoryPath, DeleteOption deleteOption = DeleteOption.FailIfNotFound)
    {
        if (deleteOption is DeleteOption.IgnoreIfNotFound && !Directory.Exists(directoryPath))
        {
            Logger.ZLogWarning($"{directoryPath} does not exists, skipping deletion");
            return true;
        }

        try
        {
            Directory.Delete(directoryPath, true);
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to delete directory {directoryPath}");
            return false;
        }
    }

    public bool TryMoveDirectory(string sourcePath, string destinationPath, bool overwrite = false)
    {
        try
        {
            if (overwrite && Directory.Exists(destinationPath))
            {
                Directory.Delete(destinationPath, true);
            }

            Directory.Move(sourcePath, destinationPath);
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to move directory from {sourcePath} to {destinationPath}");
            return false;
        }
    }

    public bool TryCopyDirectory(string sourcePath, string destinationPath)
    {
        try
        {
            CopyDirectory(sourcePath, destinationPath);
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to copy directory from {sourcePath} to {destinationPath}");
            return false;
        }
    }

    private static void CopyDirectory(string sourcePath, string destinationPath)
    {
        Directory.CreateDirectory(destinationPath);

        foreach (var file in Directory.EnumerateFiles(sourcePath))
        {
            File.Copy(file, Path.Combine(destinationPath, Path.GetFileName(file)), true);
        }

        foreach (var directory in Directory.EnumerateDirectories(sourcePath))
        {
            CopyDirectory(directory, Path.Combine(destinationPath, Path.GetFileName(directory)));
        }
    }
}