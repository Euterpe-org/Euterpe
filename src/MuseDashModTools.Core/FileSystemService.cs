using MuseDashModTools.Abstractions.Enums;

namespace MuseDashModTools.Core;

public sealed class FileSystemService : IFileSystemService
{
    #region Injections

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    #endregion Injections

    public bool CheckFileExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            return true;
        }

        Logger.Error("{File} does not exists on {Path}", Path.GetFileName(filePath), filePath);
        return false;
    }

    public bool TryDeleteFile(string filePath, DeleteOption deleteOption = DeleteOption.FailIfNotFound)
    {
        if (deleteOption == DeleteOption.IgnoreIfNotFound && !File.Exists(filePath))
        {
            Logger.Warning("{FilePath} does not exists, skipping deletion", filePath);
            return true;
        }

        try
        {
            File.Delete(filePath);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to delete file {FilePath}", filePath);
            return false;
        }
    }

    public bool CheckDirectoryExists(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            return true;
        }

        Logger.Error("{Directory} does not exists on {Path}", Path.GetDirectoryName(directoryPath), directoryPath);
        return false;
    }

    public bool TryDeleteDirectory(string directoryPath, DeleteOption deleteOption = DeleteOption.FailIfNotFound)
    {
        if (deleteOption == DeleteOption.IgnoreIfNotFound && !Directory.Exists(directoryPath))
        {
            Logger.Warning("{DirectoryPath} does not exists, skipping deletion", directoryPath);
            return true;
        }

        try
        {
            Directory.Delete(directoryPath, true);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to delete directory {DirectoryPath}", directoryPath);
            return false;
        }
    }
}