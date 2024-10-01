using Avalonia.Media;

namespace MuseDashModTools.Converters.IValueConverters;

public sealed class UninstallConverter : IValueConverter
{
    private readonly IBrush Cyan = "#75E3FF".ToBrush();
    private readonly IBrush Default = "#FEFEFE".ToBrush();
    private readonly IBrush Red = "#FD2617".ToBrush();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ModDto mod)
        {
            return Default;
        }

        if (mod.IsDuplicated)
        {
            return Cyan;
        }

        if (mod.State is ModState.Incompatible)
        {
            return Red;
        }

        return Default;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
}