using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Euterpe.Common.Extensions;

public static class StringExtensions
{
    /// <summary>
    ///     Check whether the string is null or empty
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str) => string.IsNullOrEmpty(str);

    extension(string str)
    {
        /// <summary>
        ///     Replace "\\" with "\" to normalize slashes in path
        /// </summary>
        /// <returns></returns>
        public string NormalizeSlashes() => str.Replace(@"\\", @"\");

        /// <summary>
        ///     Parse level from string
        /// </summary>
        /// <returns></returns>
        public int ParseLevel() => !int.TryParse(str, out var level) ? 0 : level;

        /// <summary>
        ///     Remove invalid chars for file names from string
        /// </summary>
        /// <returns></returns>
        public string RemoveInvalidChars()
        {
            var invalidFileNameChars = new string(Path.GetInvalidFileNameChars());
            var invalidCharRegex = new Regex($"[{Regex.Escape(invalidFileNameChars)}]");
            return invalidCharRegex.Replace(str, "_");
        }
    }
}