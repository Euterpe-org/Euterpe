using System.IO;
using System.Text.RegularExpressions;
using Avalonia.Media;

namespace MuseDashModToolsUI.Extensions;

public static class StringExtensions
{
    public static IBrush ToBrush(this string str) => (IBrush)new BrushConverter().ConvertFromString(str)!;

    public static string NormalizeNewline(this string str) => str.Replace("\\n", "\n");

    public static string RemoveInvalidChars(this string str)
    {
        var invalidFileNameChars = new string(Path.GetInvalidFileNameChars());
        var invalidCharRegex = new Regex($"[{Regex.Escape(invalidFileNameChars)}]");
        return invalidCharRegex.Replace(str, "_");
    }
}