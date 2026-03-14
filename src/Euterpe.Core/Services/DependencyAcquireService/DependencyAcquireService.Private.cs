namespace Euterpe.Core;

internal sealed partial class DependencyAcquireService
{
    private static async Task<bool> IsValidAsync(string filePath, string expectedHash)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }

        var actualHash = await SHA512Utils.HexFromPathAsync(filePath).ConfigureAwait(false);
        return actualHash == expectedHash;
    }
}