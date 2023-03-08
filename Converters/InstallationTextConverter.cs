using System;
using System.Globalization;
using Avalonia.Data.Converters;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Converters;

public class InstallationTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Mod mod)
        {
            switch (mod.State)
            {
                case UpdateState.Outdated:
                    return "Update";
                case UpdateState.Newer:
                    return "Reinstall";
                case UpdateState.Normal:
                case UpdateState.Modified:
                default:
                {
                    return mod is {State: UpdateState.Normal, IsShaMismatched: true} ? "Reinstall" : "";
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