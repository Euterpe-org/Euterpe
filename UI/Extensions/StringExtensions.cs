using Avalonia.Media;

namespace UI.Extensions;

public static class StringExtensions
{
    public static IBrush ToBrush(this string str) => (IBrush) new BrushConverter().ConvertFromString(str)!;
}