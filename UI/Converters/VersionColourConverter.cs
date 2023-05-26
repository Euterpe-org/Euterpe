using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Converters;

public class VersionColourConverter : IValueConverter
{
    private readonly IBrush Blue = "#6D96FF".ToBrush();
    private readonly IBrush Default = "#CCC".ToBrush();
    private readonly IBrush Orange = "#E19600".ToBrush();
    private readonly IBrush Purple = "#AC2AE6".ToBrush();
    private readonly IBrush Red = "#FD2617".ToBrush();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Mod mod)
        {
            if (mod.IsIncompatible)
                return Red;
            switch (mod.State)
            {
                case UpdateState.Outdated:
                    return Blue;
                case UpdateState.Newer:
                    return Purple;
                case UpdateState.Modified:
                    return Orange;
                case UpdateState.Normal:
                default:
                {
                    return Default;
                }
            }
        }

        return Default;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
}