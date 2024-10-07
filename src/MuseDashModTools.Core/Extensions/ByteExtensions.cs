namespace MuseDashModTools.Core.Extensions;

public static class ByteExtensions
{
    public static string ToHexString(this byte[] bytes) => Convert.ToHexString(bytes).ToLowerInvariant();
}