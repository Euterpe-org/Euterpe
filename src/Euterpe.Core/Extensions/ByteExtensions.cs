using System.Buffers.Text;

namespace Euterpe.Core.Extensions;

public static class ByteExtensions
{
    extension(byte[] bytes)
    {
        public string ToHexString() => Convert.ToHexString(bytes);
        public string ToHexStringLower() => Convert.ToHexStringLower(bytes);
        public string ToBase64Url() => Base64Url.EncodeToString(bytes);
    }

    extension(string value)
    {
        public byte[] FromBase64Url() => Base64Url.DecodeFromChars(value);
    }
}
