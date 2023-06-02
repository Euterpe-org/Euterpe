using Avalonia.Media;

namespace MuseDashModToolsUI.Extensions;

public static class StringExtensions
{
    public static IBrush ToBrush(this string str) => (IBrush)new BrushConverter().ConvertFromString(str)!;

    public static string Localize(this string str) => str.Replace("\\n", "\n");
}