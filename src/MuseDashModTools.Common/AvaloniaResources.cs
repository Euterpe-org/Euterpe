using Avalonia.Styling;
using MuseDashModTools.Common.Collections;

namespace MuseDashModTools.Common;

public static class AvaloniaResources
{
    public static readonly BiDictionary<string, ThemeVariant> ThemeVariants = new()
    {
        ["Light"] = ThemeVariant.Light,
        ["Dark"] = ThemeVariant.Dark
    };
}