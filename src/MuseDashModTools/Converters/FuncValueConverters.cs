using Avalonia.Media;

namespace MuseDashModTools.Converters;

public static class FuncValueConverters
{
    private const string IconPrefix = "SemiIcon";
    private static readonly IResourceService _resourceService = IocContainer.Resolve<IResourceService>();

    public static FuncValueConverter<string, StreamGeometry?> SemiIconConverter { get; } =
        new(iconKeyName => _resourceService.TryGetAppResource<StreamGeometry>($"{IconPrefix}{iconKeyName}"));
}