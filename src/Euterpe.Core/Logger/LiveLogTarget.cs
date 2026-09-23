using NLog;
using NLog.Targets;
using LogLevel = NLog.LogLevel;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Euterpe.Core.Logger;

public sealed class LiveLogTarget : Target
{
    public LiveLogTarget() => Name = "liveLog";

    protected override void Write(LogEventInfo logEvent)
    {
        var category = logEvent.LoggerName;
        if (category is "Euterpe.Services.NavigationService")
        {
            return;
        }

        var message = logEvent.FormattedMessage;
        if (message.Contains("Initialized"))
        {
            return;
        }

        OnLogMessageReceived?.Invoke(new LogMessage(
            new DateTimeOffset(logEvent.TimeStamp),
            ToMicrosoftLogLevel(logEvent.Level),
            category,
            message));
    }

    private static MicrosoftLogLevel ToMicrosoftLogLevel(LogLevel level)
    {
        if (level == LogLevel.Trace)
        {
            return MicrosoftLogLevel.Trace;
        }

        if (level == LogLevel.Debug)
        {
            return MicrosoftLogLevel.Debug;
        }

        if (level == LogLevel.Info)
        {
            return MicrosoftLogLevel.Information;
        }

        if (level == LogLevel.Warn)
        {
            return MicrosoftLogLevel.Warning;
        }

        if (level == LogLevel.Error)
        {
            return MicrosoftLogLevel.Error;
        }

        return level == LogLevel.Fatal ? MicrosoftLogLevel.Critical : MicrosoftLogLevel.None;
    }

    public event Action<LogMessage>? OnLogMessageReceived;
}
