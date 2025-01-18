using Avalonia.Styling;
using MuseDashModTools.Common.Collections;

namespace MuseDashModTools.Common;

public static class AvaloniaResources
{
    public static readonly FrozenBiDictionary<string, ThemeVariant> ThemeVariants = new BiDictionary<string, ThemeVariant>
    {
        ["Light"] = ThemeVariant.Light,
        ["Dark"] = ThemeVariant.Dark
    }.ToFrozenBiDictionary();
}