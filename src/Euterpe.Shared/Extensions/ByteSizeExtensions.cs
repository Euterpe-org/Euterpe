using System.Globalization;

namespace Euterpe.Shared.Extensions;

public static class ByteSizeExtensions
{
    extension(long bytes)
    {
        public string ToReadableSize()
        {
            var (divisor, unit) = bytes switch
            {
                >= 1 << 30 => (1 << 30, "GB"),
                >= 1 << 20 => (1 << 20, "MB"),
                _ => (1 << 10, "KB")
            };
            return string.Create(CultureInfo.InvariantCulture, $"{bytes / (double)divisor:0.#} {unit}");
        }
    }
}
