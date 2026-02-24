namespace Euterpe.Core.Logger;

public sealed class LogMessage(DateTimeOffset ts, LogLevel level, LogCategory category, string message)
{
    public DateTimeOffset Timestamp { get; } = ts;
    public LogLevel LogLevel { get; } = level;
    public LogCategory Category { get; } = category;
    public string Message { get; } = message;

    public string LevelAbbreviation { get; } = level switch
    {
        LogLevel.Trace => "TRC",
        LogLevel.Debug => "DBG",
        LogLevel.Information => "INF",
        LogLevel.Warning => "WRN",
        LogLevel.Error => "ERR",
        LogLevel.Critical => "CRT",
        _ => "???"
    };

    public override string ToString() => $"[{Timestamp:HH:mm:ss}] [{LogLevel}] - {Message}";
}