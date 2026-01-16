using System.Security.Cryptography;

namespace Euterpe.Core.Utils;

public static class SHA512Utils
{
    public static string HexFromBytes(byte[] bytes) =>
        SHA512.HashData(bytes).ToHexString();

    public static string HexFromPath(string filePath) =>
        SHA512.HashData(File.ReadAllBytes(filePath)).ToHexString();

    public static async Task<string> HexFromPathAsync(string filePath)
    {
        var stream = File.OpenRead(filePath);
        await using (stream.ConfigureAwait(false))
        {
            var hash = await SHA512.HashDataAsync(stream).ConfigureAwait(false);
            return hash.ToHexString();
        }
    }

    public static string HexLowerFromBytes(byte[] bytes) =>
        SHA512.HashData(bytes).ToHexStringLower();

    public static string HexLowerFromPath(string filePath) =>
        SHA512.HashData(File.ReadAllBytes(filePath)).ToHexStringLower();

    public static async Task<string> HexLowerFromPathAsync(string filePath)
    {
        var stream = File.OpenRead(filePath);
        await using (stream.ConfigureAwait(false))
        {
            var hash = await SHA512.HashDataAsync(stream).ConfigureAwait(false);
            return hash.ToHexStringLower();
        }
    }
}