using Avalonia.Media;

namespace MuseDashModTools.Converters;

public static class FuncValueConverters
{
    private const string IconPrefix = "SemiIcon";
    private static readonly IResourceService _resourceService = App.Container.Resolve<IResourceService>();

    public static FuncValueConverter<string, StreamGeometry?> SemiIconConverter { get; } = new(iconKeyName =>
    {
        if (iconKeyName.IsNullOrEmpty() || _resourceService.TryGetAppResource<StreamGeometry>($"{IconPrefix}{iconKeyName}") is not { } result)
        {
            return null;
        }

        return result;
    });

    public static FuncValueConverter<string?, string?> ChartSceneConverter { get; } = new(scene => scene switch
    {
        "scene_01" => XAML_Scene_SpaceStation,
        "scene_02" => XAML_Scene_Retrocity,
        "scene_03" => XAML_Scene_Castle,
        "scene_04" => XAML_Scene_RainyNight,
        "scene_05" => XAML_Scene_Candyland,
        "scene_06" => XAML_Scene_Oriental,
        "scene_07" => XAML_Scene_LetsGroove,
        "scene_08" => XAML_Scene_Touhou,
        "scene_09" => XAML_Scene_DJMAX,
        _ => scene
    });
}