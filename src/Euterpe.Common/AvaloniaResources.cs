using Avalonia.Styling;
using Euterpe.Common.Collections;

namespace Euterpe.Common;

public static class AvaloniaResources
{
    public static readonly FrozenBiDictionary<string, ThemeVariant> ThemeVariants = new BiDictionary<string, ThemeVariant>
    {
        ["Light"] = ThemeVariant.Light,
        ["Dark"] = ThemeVariant.Dark
    }.ToFrozenBiDictionary();
}