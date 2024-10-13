namespace MuseDashModTools.Core.Extensions;

public static class ByteExtensions
{
    public static string ToLowerHexString(this byte[] bytes) => Convert.ToHexString(bytes).ToLowerInvariant();
}