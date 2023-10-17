using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

// ReSharper disable InconsistentNaming

namespace MuseDashModToolsUI.Converters.IValueConverters;

public class VersionColourConverter : IValueConverter
{
    private readonly IBrush Blue = "#6D96FF".ToBrush();
    private readonly IBrush Default = "#CCC".ToBrush();
    private readonly IBrush Orange = "#E19600".ToBrush();
    private readonly IBrush Purple = "#A228D9".ToBrush();
    private readonly IBrush Red = "#FD2617".ToBrush();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Mod mod) return Default;

        if (mod.IsIncompatible)
            return Red;

        return mod.State switch
        {
            UpdateState.Outdated => Blue,
            UpdateState.Newer => Purple,
            UpdateState.Modified => Orange,
            UpdateState.Normal => Default,
            _ => Default
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
}