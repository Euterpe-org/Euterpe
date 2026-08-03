namespace Euterpe.Core;

internal sealed partial class FileSystemService
{
    public Stream? TryOpenReadFile(string filePath)
    {
        try
        {
            return File.OpenRead(filePath);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to open file {FilePath}", filePath);
            return null;
        }
    }

    public Stream? TryOpenSharedReadFile(string filePath)
    {
        try
        {
            return new FileStream(filePath, new FileStreamOptions
            {
                Mode = FileMode.Open,
                Access = FileAccess.Read,
                Share = FileShare.ReadWrite | FileShare.Delete,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan
            });
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to open shared file {FilePath}", filePath);
            return null;
        }
    }

    public Stream? TryCreateTemporaryFile()
    {
        var temporaryFilePath = Path.Combine(Path.GetTempPath(), $"{AppName}-{Guid.CreateVersion7():N}.tmp");
        try
        {
            return new FileStream(temporaryFilePath, new FileStreamOptions
            {
                Mode = FileMode.CreateNew,
                Access = FileAccess.ReadWrite,
                Share = FileShare.None,
                Options = FileOptions.Asynchronous | FileOptions.DeleteOnClose | FileOptions.SequentialScan
            });
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to create temporary file {FilePath}", temporaryFilePath);
            return null;
        }
    }
}
