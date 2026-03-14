using static Euterpe.Core.Logger.LoggingConstants;

namespace Euterpe.Core.Logger;

public sealed record LogMessage(DateTimeOffset Timestamp, LogLevel LogLevel, LogCategory Category, string Message)
{
    public string LogLevelAbbreviation { get; } = LevelAbbreviations[(int)LogLevel];
}