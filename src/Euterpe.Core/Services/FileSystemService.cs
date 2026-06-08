namespace Euterpe.Core;

internal sealed class FileSystemService : IFileSystemService
{
    #region Injections

    public required ILogger<FileSystemService> Logger { get; init; }

    #endregion Injections

    public void DeleteFile(string filePath, DeleteOption deleteOption = DeleteOption.FailIfNotFound)
    {
        if (deleteOption is DeleteOption.IgnoreIfNotFound && !File.Exists(filePath))
        {
            return;
        }

        File.Delete(filePath);
    }

    public bool TryDeleteFile(string filePath, DeleteOption deleteOption = DeleteOption.FailIfNotFound)
    {
        try
        {
            DeleteFile(filePath, deleteOption);
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

    public void DeleteDirectory(string directoryPath, DeleteOption deleteOption = DeleteOption.FailIfNotFound)
    {
        if (deleteOption is DeleteOption.IgnoreIfNotFound && !Directory.Exists(directoryPath))
        {
            return;
        }

        Directory.Delete(directoryPath, true);
    }

    public bool TryDeleteDirectory(string directoryPath, DeleteOption deleteOption = DeleteOption.FailIfNotFound)
    {
        try
        {
            DeleteDirectory(directoryPath, deleteOption);
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

    public void CopyDirectory(string sourcePath, string destinationPath)
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
