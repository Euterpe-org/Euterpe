using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Converters;

public class DuplicateConverter : IValueConverter
{
    private readonly IBrush Default = "#E6E6E6".ToBrush();
    private readonly IBrush Cyan = "#75E3FF".ToBrush();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Mod mod) return Default;
        
        return mod.IsDuplicated ? Cyan : Default;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}