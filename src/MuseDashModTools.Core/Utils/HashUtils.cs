using System.Security.Cryptography;

namespace MuseDashModTools.Core.Utils;

public static class HashUtils
{
    public static string ComputeSHA256HashFromBytes(byte[] bytes) =>
        SHA256.HashData(bytes).ToHexStringLower();

    public static string ComputeSHA256HashFromPath(string filePath) =>
        SHA256.HashData(File.ReadAllBytes(filePath)).ToHexStringLower();

    public static async Task<string> ComputeSHA256HashFromPathAsync(string filePath) =>
        SHA256.HashData(await File.ReadAllBytesAsync(filePath).ConfigureAwait(false)).ToHexStringLower();
}