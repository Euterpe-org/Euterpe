using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace MuseDashModToolsUI.Converters;

public class UninstallConverter : IValueConverter
{
    private readonly IBrush Cyan = "#75E3FF".ToBrush();
    private readonly IBrush Default = "#FEFEFE".ToBrush();
    private readonly IBrush Red = "#FD2617".ToBrush();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Mod mod) return Default;
        if (mod.IsIncompatible) return Red;
        if (mod.IsDuplicated) return Cyan;
        return Default;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
}