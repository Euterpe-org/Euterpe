using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Converters;

public class VersionColourConverter : IValueConverter
{
    private readonly IBrush Default = "#BFBFBF".ToBrush();
    private readonly IBrush Blue = "#82AAFF".ToBrush();
    private readonly IBrush Red = "#FD2617".ToBrush();
    private readonly IBrush Purple = "#BE3CF0".ToBrush();
    private readonly IBrush Yellow = "#E19600".ToBrush();

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
                    return Yellow;
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