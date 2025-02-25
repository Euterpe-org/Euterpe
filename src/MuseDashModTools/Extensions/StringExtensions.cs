using Avalonia.Media;

namespace MuseDashModTools.Extensions;

public static class StringExtensions
{
    /// <summary>
    ///     Replace "\\n" with "\n" to normalize newline
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string NormalizeNewline(this string str) => str.Replace("\\n", "\n");

    /// <summary>
    ///     Convert string to IBrush
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static IBrush ToBrush(this string str) => (IBrush)new BrushConverter().ConvertFromString(str)!;
}