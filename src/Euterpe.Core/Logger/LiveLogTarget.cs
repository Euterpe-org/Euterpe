using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;
using NLog;
using NLog.Targets;

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

    private static MicrosoftLogLevel ToMicrosoftLogLevel(NLog.LogLevel level)
    {
        if (level == NLog.LogLevel.Trace)
        {
            return MicrosoftLogLevel.Trace;
        }

        if (level == NLog.LogLevel.Debug)
        {
            return MicrosoftLogLevel.Debug;
        }

        if (level == NLog.LogLevel.Info)
        {
            return MicrosoftLogLevel.Information;
        }

        if (level == NLog.LogLevel.Warn)
        {
            return MicrosoftLogLevel.Warning;
        }

        if (level == NLog.LogLevel.Error)
        {
            return MicrosoftLogLevel.Error;
        }

        return level == NLog.LogLevel.Fatal ? MicrosoftLogLevel.Critical : MicrosoftLogLevel.None;
    }

    public event Action<LogMessage>? OnLogMessageReceived;
}
