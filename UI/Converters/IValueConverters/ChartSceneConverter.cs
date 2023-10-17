using System.Globalization;
using Avalonia.Data.Converters;

namespace MuseDashModToolsUI.Converters.IValueConverters;

public class ChartSceneConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string scene) return value;
        return scene switch
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
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
}