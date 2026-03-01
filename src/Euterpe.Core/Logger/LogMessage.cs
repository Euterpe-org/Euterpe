using static Euterpe.Core.Logger.LoggingConstants;

namespace Euterpe.Core.Logger;

public sealed class LogMessage(DateTimeOffset ts, LogLevel level, LogCategory category, string message)
{
    public DateTimeOffset Timestamp { get; } = ts;
    public LogLevel LogLevel { get; } = level;
    public LogCategory Category { get; } = category;
    public string Message { get; } = message;
    public string LogLevelAbbreviation { get; } = LevelAbbreviations[(int)level];
}