using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using UI.Extensions;
using UI.Models;

namespace UI.Converters;

public class ReinstallConverter : IValueConverter
{
    private readonly IBrush Default = "#bbb".ToBrush();
    private readonly IBrush Red = "#fd2617".ToBrush();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Mod mod)
        {
            return mod.IsIncompatible ? Red : Default;
        }

        return Default;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
}