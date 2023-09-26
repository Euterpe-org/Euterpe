using System.Globalization;
using Avalonia.Data.Converters;

namespace MuseDashModToolsUI.Converters;

public class InstallationTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Mod mod)
            switch (mod.State)
            {
                case UpdateState.Outdated:
                    return XAML_Update_Mod;
                case UpdateState.Newer:
                case UpdateState.Modified:
                    return XAML_Reinstall_Mod;
                case UpdateState.Normal:
                default:
                {
                    return string.Empty;
                }
            }

        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
}