using static Euterpe.Core.Logger.AnsiEscapeColors;

namespace Euterpe.Core.Logger;

internal static class LoggingConstants
{
    internal static readonly string[] LevelAbbreviations =
    [
        "TRC",
        "DBG",
        "INF",
        "WRN",
        "ERR",
        "CRT",
        "NON"
    ];

    internal static readonly string[] LevelColors =
    [
        Blue,
        BrightGreen,
        BrightCyan,
        Yellow,
        Red,
        BrightRed,
        White
    ];
}