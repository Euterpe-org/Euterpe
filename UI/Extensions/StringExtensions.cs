using System.IO;
using System.Text.RegularExpressions;
using Avalonia.Media;

namespace MuseDashModToolsUI.Extensions;

public static class StringExtensions
{
    /// <summary>
    ///     Using span and IndexOf to check whether string contains value, slightly faster than Contains
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool ContainsString(this string str, string value) => str.AsSpan().IndexOf(value.AsSpan(), StringComparison.Ordinal) != -1;

    /// <summary>
    ///     Replace "\\n" with "\n" to normalize newline
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string NormalizeNewline(this string str) => str.Replace("\\n", "\n");

    /// <summary>
    ///     Parse level from string
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static int ParseLevel(this string str) => !int.TryParse(str, out var level) ? 0 : level;

    /// <summary>
    ///     Remove invalid chars for file names from string
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string RemoveInvalidChars(this string str)
    {
        var invalidFileNameChars = new string(Path.GetInvalidFileNameChars());
        var invalidCharRegex = new Regex($"[{Regex.Escape(invalidFileNameChars)}]");
        return invalidCharRegex.Replace(str, "_");
    }

    /// <summary>
    ///     Convert string to IBrush
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static IBrush ToBrush(this string str) => (IBrush)new BrushConverter().ConvertFromString(str)!;
}