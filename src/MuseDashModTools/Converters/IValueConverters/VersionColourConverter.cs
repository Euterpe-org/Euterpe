using Avalonia.Media;

namespace MuseDashModTools.Converters.IValueConverters;

public sealed class VersionColourConverter : IValueConverter
{
    private readonly IBrush Blue = "#6D96FF".ToBrush();
    private readonly IBrush Default = "#CCC".ToBrush();
    private readonly IBrush Orange = "#E19600".ToBrush();
    private readonly IBrush Purple = "#A228D9".ToBrush();
    private readonly IBrush Red = "#FD2617".ToBrush();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ModDto mod)
        {
            return Default;
        }

        return mod.State switch
        {
            ModState.Incompatible => Red,
            ModState.Outdated => Blue,
            ModState.Normal => Default,
            ModState.Modified => Orange,
            ModState.Newer => Purple,
            _ => throw new UnreachableException()
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
}