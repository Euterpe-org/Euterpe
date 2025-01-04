using Avalonia.Media;

namespace MuseDashModTools.Converters;

public static class FuncValueConverters
{
    private const string IconPrefix = "SemiIcon";
    private static readonly IResourceService _resourceService = App.Container.Resolve<IResourceService>();
    public static FuncValueConverter<bool, int> IconSizeConverter { get; } = new(b => b ? 24 : 16);

    public static FuncValueConverter<string, StreamGeometry?> SemiIconConverter { get; } = new(iconKeyName =>
    {
        if (iconKeyName.IsNullOrEmpty() || _resourceService.TryGetAppResource<StreamGeometry>($"{IconPrefix}{iconKeyName}") is not { } result)
        {
            return null;
        }

        return result;
    });
}