using System.Buffers;
using Utf8StringInterpolation;
using static Euterpe.Core.Logger.AnsiEscapeColors;

namespace Euterpe.Core.Logger;

internal sealed class LogConsoleFormatter : IZLoggerFormatter
{
    private static readonly Dictionary<LogLevel, string> _levelPrefixes = new()
    {
        [LogLevel.Trace] = $"{Blue}[TRC]{Reset}",
        [LogLevel.Debug] = $"{BrightGreen}[DBG]{Reset}",
        [LogLevel.Information] = $"{BrightCyan}[INF]{Reset}",
        [LogLevel.Warning] = $"{Yellow}[WRN]{Reset}",
        [LogLevel.Error] = $"{Red}[ERR]{Reset}",
        [LogLevel.Critical] = $"{BrightRed}[CRT]{Reset}"
    };

    public void FormatLogEntry(IBufferWriter<byte> writer, IZLoggerEntry entry)
    {
        using var utf8Writer = new Utf8StringWriter<IBufferWriter<byte>>(writer);

        utf8Writer.Append(_levelPrefixes[entry.LogInfo.LogLevel]);
        utf8Writer.AppendUtf8("("u8);
        utf8Writer.AppendUtf8(entry.LogInfo.Category.Utf8Span[17..]);
        utf8Writer.AppendUtf8(") "u8);
        utf8Writer.Append(entry.ToString());

        if (entry.LogInfo.Exception is not { } ex)
        {
            return;
        }

        utf8Writer.AppendLine();
        utf8Writer.Append(ex.ToString());
    }

    public bool WithLineBreak => true;
}