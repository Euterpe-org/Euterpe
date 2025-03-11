namespace MuseDashModTools.Generators.Extensions;

public static class StringExtensions
{
    public static string EscapeXmlDoc(this string text) =>
        text.Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");

    public static string GetValidIdentifier(this string text) =>
        new(text.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
}