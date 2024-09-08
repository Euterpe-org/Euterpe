using System.Text;

namespace MuseDashModToolsUI.Extensions;

public static class ByteExtensions
{
    public static string ToHexString(this byte[] bytes)
    {
        var sb = new StringBuilder();
        foreach (var b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }

    public static string ToString(this byte[] bytes, string format)
    {
        var sb = new StringBuilder();
        foreach (var b in bytes)
        {
            sb.Append(b.ToString(format));
        }

        return sb.ToString();
    }
}