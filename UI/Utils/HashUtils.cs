using System.Security.Cryptography;

namespace MuseDashModTools.Utils;

public static class HashUtils
{
    public static string ComputeSHA256HashFromPath(string filePath) =>
        SHA256.HashData(File.ReadAllBytes(filePath)).ToHexString();

    public async static Task<string> ComputeSHA256HashFromPathAsync(string filePath) =>
        SHA256.HashData(await File.ReadAllBytesAsync(filePath).ConfigureAwait(false)).ToHexString();
}