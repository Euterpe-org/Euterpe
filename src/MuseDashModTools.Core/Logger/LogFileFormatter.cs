using System.Buffers;
using Utf8StringInterpolation;

namespace MuseDashModTools.Core.Logger;

public sealed class LogFileFormatter : IZLoggerFormatter
{
    public void FormatLogEntry(IBufferWriter<byte> writer, IZLoggerEntry entry)
    {
        using var utf8Writer = new Utf8StringWriter<IBufferWriter<byte>>(writer);

        utf8Writer.AppendLine($"[{entry.LogInfo.Timestamp.Local:HH:mm:ss.fff zzz}] [{entry.LogInfo.LogLevel}] ({entry.LogInfo.Category})");
        utf8Writer.AppendLine(entry.ToString());

        if (entry.LogInfo.Exception is { } ex)
        {
            utf8Writer.AppendLine(ex.ToString());
        }
    }

    public bool WithLineBreak => true;
}