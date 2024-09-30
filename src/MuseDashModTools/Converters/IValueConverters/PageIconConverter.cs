using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace MuseDashModTools.Converters.IValueConverters;

public sealed class PageIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string s || !GetCurrentApplication().TryGetResource(s, out var result))
        {
            return null;
        }

        return result as StreamGeometry;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
}