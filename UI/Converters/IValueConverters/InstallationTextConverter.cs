using System.Globalization;
using Avalonia.Data.Converters;

namespace MuseDashModToolsUI.Converters.IValueConverters;

public class InstallationTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Mod mod) return string.Empty;

        return mod.State switch
        {
            UpdateState.Outdated => XAML_Update_Mod,
            UpdateState.Newer or UpdateState.Modified => XAML_Reinstall_Mod,
            UpdateState.Normal => string.Empty,
            _ => string.Empty
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
}