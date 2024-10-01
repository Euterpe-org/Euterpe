using Avalonia.Media;

namespace MuseDashModTools.Converters;

public static class FuncValueConverters
{
    public static FuncValueConverter<bool, int> IconSizeConverter { get; } = new(b => b ? 20 : 16);

    public static FuncValueConverter<string, StreamGeometry?> PageIconConverter { get; } = new(str =>
    {
        if (str.IsNullOrEmpty() || !GetCurrentApplication().TryGetResource(str, out var result))
        {
            return null;
        }

        return result as StreamGeometry;
    });
}