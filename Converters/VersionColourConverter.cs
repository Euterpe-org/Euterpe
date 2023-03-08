using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Converters;

public class VersionColourConverter : IValueConverter
{
    private readonly IBrush Default = "#bbb".ToBrush();
    private readonly IBrush Red = "#c80000".ToBrush();
    private readonly IBrush Purple = "#a000e6".ToBrush();
    private readonly IBrush Yellow = "#e19600".ToBrush();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Mod mod)
        {
            switch (mod.State)
            {
                case UpdateState.Outdated:
                    return Red;
                case UpdateState.Newer:
                    return Purple;
                case UpdateState.Normal:
                case UpdateState.Modified:
                default:
                {
                    if (mod is {State: UpdateState.Normal, IsShaMismatched: true})
                    {
                        return Yellow;
                    }

                    break;
                }
            }
        }

        return Default;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}