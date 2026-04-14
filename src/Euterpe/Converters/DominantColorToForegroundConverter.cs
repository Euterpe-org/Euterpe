using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Euterpe.Converters;

/// <summary>
/// Converts a hex dominant color string (e.g. "#a0b0c0") to a contrasting
/// foreground brush (white or black) based on perceived luminance.
/// Used by the screenshots carousel so navigation controls remain readable
/// regardless of the image's dominant color.
/// </summary>
public sealed class DominantColorToForegroundConverter : IValueConverter
{
    public static readonly DominantColorToForegroundConverter Instance = new();

    private static readonly SolidColorBrush LightBrush = new(Colors.White);
    private static readonly SolidColorBrush DarkBrush = new(Color.FromRgb(30, 30, 46));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string hex || hex.Length < 7 || hex[0] != '#')
            return LightBrush;

        try
        {
            var r = System.Convert.ToByte(hex.Substring(1, 2), 16);
            var g = System.Convert.ToByte(hex.Substring(3, 2), 16);
            var b = System.Convert.ToByte(hex.Substring(5, 2), 16);

            // Perceived luminance (ITU-R BT.709)
            var luminance = 0.2126 * r + 0.7152 * g + 0.0722 * b;
            return luminance > 140 ? DarkBrush : LightBrush;
        }
        catch
        {
            return LightBrush;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
