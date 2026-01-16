namespace Euterpe.Core.Logger;

internal static class AnsiEscapeColors
{
    internal const string Reset = "\e[0m";
    internal const string Bold = "\e[1m";

    internal const string Black = "\e[30m";
    internal const string Red = "\e[31m";
    internal const string Green = "\e[32m";
    internal const string Yellow = "\e[33m";
    internal const string Blue = "\e[34m";
    internal const string Magenta = "\e[35m";
    internal const string Cyan = "\e[36m";
    internal const string White = "\e[37m";

    internal const string BrightBlack = "\e[30;1m";
    internal const string BrightRed = "\e[31;1m";
    internal const string BrightGreen = "\e[32;1m";
    internal const string BrightYellow = "\e[33;1m";
    internal const string BrightBlue = "\e[34;1m";
    internal const string BrightMagenta = "\e[35;1m";
    internal const string BrightCyan = "\e[36;1m";
    internal const string BrightWhite = "\e[37;1m";
}