using System.Buffers;
using Utf8StringInterpolation;

namespace MuseDashModTools.Core.Logger;

internal sealed class LogMemoryFormatter : IZLoggerFormatter
{
    public void FormatLogEntry(IBufferWriter<byte> writer, IZLoggerEntry entry)
    {
        using var utf8Writer = new Utf8StringWriter<IBufferWriter<byte>>(writer);

        utf8Writer.Append($"[{entry.LogInfo.Timestamp.Local:HH:mm:ss}] [{entry.LogInfo.LogLevel}]");
        utf8Writer.AppendUtf8(" - "u8);
        utf8Writer.Append(entry.ToString());
    }

    public bool WithLineBreak => true;
}