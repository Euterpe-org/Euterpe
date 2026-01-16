using Avalonia.Media;

namespace Euterpe.Extensions;

public static class StringExtensions
{
    extension(string str)
    {
        /// <summary>
        ///     Replace "\\n" with "\n" to normalize newline
        /// </summary>
        /// <returns></returns>
        public string NormalizeNewline() => str.Replace("\\n", "\n");

        /// <summary>
        ///     Convert string to IBrush
        /// </summary>
        /// <returns></returns>
        public IBrush ToBrush() => (IBrush)new BrushConverter().ConvertFromString(str)!;
    }
}