using System.Buffers;
using Utf8StringInterpolation;
using static Euterpe.Core.Logger.AnsiEscapeColors;
using static Euterpe.Core.Logger.LoggingConstants;

namespace Euterpe.Core.Logger;

internal sealed class LogConsoleFormatter : IZLoggerFormatter
{
    public void FormatLogEntry(IBufferWriter<byte> writer, IZLoggerEntry entry)
    {
        using var utf8Writer = new Utf8StringWriter<IBufferWriter<byte>>(writer);

        var logLevel = (int)entry.LogInfo.LogLevel;
        utf8Writer.Append($"{LevelColors[logLevel]}[{LevelAbbreviations[logLevel]}]{Reset}");
        utf8Writer.AppendUtf8("("u8);
        utf8Writer.AppendUtf8(entry.LogInfo.Category.Utf8Span);
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