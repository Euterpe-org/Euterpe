namespace MuseDashModTools.Core;

internal sealed class FileSystemService : IFileSystemService
{
    #region Injections

    [UsedImplicitly]
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

    public bool TryDeleteFile(string filePath, DeleteOption deleteOption = DeleteOption.FailIfNotFound)
    {
        if (deleteOption == DeleteOption.IgnoreIfNotFound && !File.Exists(filePath))
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
            Logger.ZLogError(ex, $"Failed to delete file {filePath}");
            return false;
        }
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

    public bool TryDeleteDirectory(string directoryPath, DeleteOption deleteOption = DeleteOption.FailIfNotFound)
    {
        if (deleteOption == DeleteOption.IgnoreIfNotFound && !Directory.Exists(directoryPath))
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
            Logger.ZLogError(ex, $"Failed to delete directory {directoryPath}");
            return false;
        }
    }
}