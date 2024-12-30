using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace MuseDashModTools.Common.Extensions;

public static class StringExtensions
{
    /// <summary>
    ///     Check whether the string is null or empty
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str) => string.IsNullOrEmpty(str);

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
}