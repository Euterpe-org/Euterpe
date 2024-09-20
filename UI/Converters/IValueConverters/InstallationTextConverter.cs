using System.Globalization;
using Avalonia.Data.Converters;

namespace MuseDashModTools.Converters.IValueConverters;

public sealed class InstallationTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ModDto mod)
        {
            return string.Empty;
        }

        return mod.State switch
        {
            ModState.Outdated => XAML_Update_Mod,
            ModState.Newer or ModState.Modified => XAML_Reinstall_Mod,
            ModState.Normal => string.Empty,
            _ => string.Empty
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
}