using Avalonia.Media;

namespace MuseDashModTools.Converters;

public static class FuncValueConverters
{
    private static readonly IResourceService _resourceService = App.Container.Resolve<IResourceService>();
    public static FuncValueConverter<bool, int> IconSizeConverter { get; } = new(b => b ? 24 : 16);

    public static FuncValueConverter<string, StreamGeometry?> PageIconConverter { get; } = new(str =>
    {
        if (str.IsNullOrEmpty() || _resourceService.TryGetAppResource<StreamGeometry>(str) is not { } result)
        {
            return null;
        }

        return result;
    });
}