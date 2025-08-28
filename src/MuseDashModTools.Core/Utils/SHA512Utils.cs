using System.Security.Cryptography;

namespace MuseDashModTools.Core.Utils;

public static class SHA512Utils
{
    public static string HexFromBytes(byte[] bytes) =>
        SHA512.HashData(bytes).ToHexString();

    public static string HexFromPath(string filePath) =>
        SHA512.HashData(File.OpenRead(filePath)).ToHexString();

    public static async Task<string> HexFromPathAsync(string filePath) =>
        (await SHA512.HashDataAsync(File.OpenRead(filePath)).ConfigureAwait(false)).ToHexString();

    public static string HexLowerFromBytes(byte[] bytes) =>
        SHA512.HashData(bytes).ToHexStringLower();

    public static string HexLowerFromPath(string filePath) =>
        SHA512.HashData(File.ReadAllBytes(filePath)).ToHexStringLower();

    public static async Task<string> HexLowerFromPathAsync(string filePath) =>
        (await SHA512.HashDataAsync(File.OpenRead(filePath)).ConfigureAwait(false)).ToHexStringLower();
}