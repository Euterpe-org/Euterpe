namespace MuseDashModTools.Converters.IValueConverters;

public sealed class ModFilterTypeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Array enumValues)
        {
            return enumValues.Cast<ModFilterType>()
                .Select(x => x.ToString());
        }

        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str && Enum.TryParse<ModFilterType>(str, out var result))
        {
            return result;
        }
        return null;
    }
}