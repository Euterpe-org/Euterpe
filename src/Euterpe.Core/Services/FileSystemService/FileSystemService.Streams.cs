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
            Logger.LogWarning(ex, $"Failed to open file {filePath}");
            return null;
        }
    }
}
