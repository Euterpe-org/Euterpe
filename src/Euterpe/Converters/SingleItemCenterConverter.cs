using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Layout;

namespace Euterpe.Converters;

/// <summary>
/// Returns <see cref="HorizontalAlignment.Center"/> when the bound count is 1,
/// otherwise <see cref="HorizontalAlignment.Left"/>.
/// </summary>
public sealed class SingleItemCenterConverter : IValueConverter
{
    public static readonly SingleItemCenterConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var count = value is int n ? n : 0;
        return count == 1 ? HorizontalAlignment.Center : HorizontalAlignment.Left;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
