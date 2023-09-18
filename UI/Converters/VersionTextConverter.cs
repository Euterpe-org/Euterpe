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
            if (mod.IsIncompatible)
                return string.Format(XAML_Mod_Incompatible, mod.Name, mod.Version);
            switch (mod.State)
            {
                case UpdateState.Outdated:
                    return string.Format(XAML_Mod_Outdated, mod.LocalVersion, mod.Version);
                case UpdateState.Newer:
                    return string.Format(XAML_Mod_Newer, mod.LocalVersion);
                case UpdateState.Modified:
                    return string.Format(XAML_Mod_Modified, mod.LocalVersion);
                case UpdateState.Normal:
                default:
                {
                    return mod is { State: UpdateState.Normal, IsLocal: true }
                        ? string.Format(XAML_Mod_Normal, mod.LocalVersion)
                        : null;
                }
            }
        }

        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
}