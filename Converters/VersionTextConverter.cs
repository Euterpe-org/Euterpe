using System;
using System.Globalization;
using Avalonia.Data.Converters;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Converters;

public class VersionTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Mod mod)
        {
            switch (mod.State)
            {
                case UpdateState.Outdated:
                    return $"{mod.LocalVersion} (Has a newer version: {mod.Version})";
                case UpdateState.Newer:
                    return $"{mod.LocalVersion} (WOW MOD DEV)";
                case UpdateState.Normal:
                case UpdateState.Modified:
                default:
                {
                    return mod is {State: UpdateState.Normal, IsShaMismatched: true} ? $"{mod.LocalVersion} (Modified)" : mod.LocalVersion;
                }
            }
        }

        return "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}