namespace Euterpe.Core;

internal sealed partial class FileSystemService
{
    private const int MaxAvailablePathAttempts = 8;

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
            Logger.LogWarning(ex, $"Failed to delete directory {directoryPath}");
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
            Logger.LogWarning(ex, $"Failed to move directory from {sourcePath} to {destinationPath}");
            return false;
        }
    }

    public bool TryMoveDirectoryToAvailablePath(
        string sourcePath,
        string desiredPath,
        out string finalPath)
    {
        var parent = Path.GetDirectoryName(desiredPath);
        var candidate = desiredPath;

        for (var attempt = 0; attempt < MaxAvailablePathAttempts; attempt++)
        {
            try
            {
                if (!parent.IsNullOrEmpty())
                {
                    Directory.CreateDirectory(parent);
                }

                Directory.Move(sourcePath, candidate);
                finalPath = candidate;
                return true;
            }
            catch (IOException) when (Directory.Exists(candidate))
            {
                candidate = $"{desiredPath}-{Guid.NewGuid().ToString("N")[..8]}";
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, $"Failed to move directory from {sourcePath} to {candidate}");
                finalPath = string.Empty;
                return false;
            }
        }

        Logger.LogWarning($"Exhausted available-path attempts moving directory from {sourcePath} to {desiredPath}");
        finalPath = string.Empty;
        return false;
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
