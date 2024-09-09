using System.Globalization;
using Avalonia.Data.Converters;

namespace MuseDashModToolsUI.Converters.IValueConverters;

public sealed class VersionTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ModDto mod)
        {
            return string.Empty;
        }

        return mod.State switch
        {
            ModState.Incompatible => string.Format(XAML_Mod_Incompatible, mod.Name, mod.Version),
            ModState.Outdated => string.Format(XAML_Mod_Outdated, mod.LocalVersion, mod.Version),
            ModState.Normal => mod.IsLocal ? string.Format(XAML_Mod_Normal, mod.LocalVersion) : null,
            ModState.Modified => string.Format(XAML_Mod_Modified, mod.LocalVersion),
            ModState.Newer => string.Format(XAML_Mod_Newer, mod.LocalVersion),
            _ => throw new UnreachableException()
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
}