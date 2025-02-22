using System.Buffers;
using Utf8StringInterpolation;
using static MuseDashModTools.Core.Logger.AnsiEscapeColors;

namespace MuseDashModTools.Core.Logger;

public sealed class LogConsoleFormatter : IZLoggerFormatter
{
    public void FormatLogEntry(IBufferWriter<byte> writer, IZLoggerEntry entry)
    {
        using var utf8Writer = new Utf8StringWriter<IBufferWriter<byte>>(writer);
        switch (entry.LogInfo.LogLevel)
        {
            case LogLevel.Trace:
                utf8Writer.Append($"{Blue}[TRC]{Reset}");
                break;
            case LogLevel.Debug:
                utf8Writer.Append($"{BrightGreen}[DBG]{Reset}");
                break;
            case LogLevel.Information:
                utf8Writer.Append($"{BrightCyan}[INF]{Reset}");
                break;
            case LogLevel.Warning:
                utf8Writer.Append($"{Yellow}[WRN]{Reset}");
                break;
            case LogLevel.Error:
                utf8Writer.Append($"{Red}[ERR]{Reset}");
                break;
            case LogLevel.Critical:
                utf8Writer.Append($"{BrightRed}[CRT]{Reset}");
                break;
            case LogLevel.None:
                utf8Writer.Append("[NON]");
                break;
            default:
                throw new UnreachableException();
        }

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