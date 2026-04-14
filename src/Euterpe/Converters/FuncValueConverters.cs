using Avalonia.Layout;
using Avalonia.Media;

namespace Euterpe.Converters;

public static class FuncValueConverters
{
    private const string IconPrefix = "SemiIcon";
    private static readonly IResourceService _resourceService = IocContainer.Resolve<IResourceService>();

    public static FuncValueConverter<string, StreamGeometry?> SemiIconConverter { get; } =
        new(iconKeyName => _resourceService.TryGetAppResource<StreamGeometry>($"{IconPrefix}{iconKeyName}"));

    public static FuncValueConverter<int, HorizontalAlignment> HorizontalAlignmentConverter { get; } =
        new(count => count is 1 ? HorizontalAlignment.Center : HorizontalAlignment.Left);

    public static FuncValueConverter<string?, IBrush?> HexColorToBrushConverter { get; } =
        new(hex => Color.TryParse(hex, out var color) ? new SolidColorBrush(color) : null);
}