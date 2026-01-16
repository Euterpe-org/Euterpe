namespace Euterpe.Core.Extensions;

public static class ByteExtensions
{
    extension(byte[] bytes)
    {
        public string ToHexString() => Convert.ToHexString(bytes);
        public string ToHexStringLower() => Convert.ToHexStringLower(bytes);
    }
}