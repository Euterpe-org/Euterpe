using System.Security.Cryptography;

namespace MuseDashModTools.Core.Utils;

public static class HashUtils
{
    public static string ComputeSHA256HashFromPath(string filePath) =>
        SHA256.HashData(File.ReadAllBytes(filePath)).ToLowerHexString();

    public async static Task<string> ComputeSHA256HashFromPathAsync(string filePath) =>
        SHA256.HashData(await File.ReadAllBytesAsync(filePath).ConfigureAwait(false)).ToLowerHexString();
}