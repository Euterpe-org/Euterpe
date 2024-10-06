using Serilog;
using Serilog.Events;
using TUnit.Core.Logging;

namespace MuseDashModTools.Tests;

public sealed class TestLogger : ILogger
{
    private readonly TUnitLogger _tUnitLogger;

    public TestLogger()
    {
        if (TestContext.Current is null)
        {
            throw new InvalidOperationException("Test Context is null");
        }

        _tUnitLogger = TestContext.Current.GetDefaultLogger();
    }

    public TestLogger(TUnitLogger tUnitLogger) => _tUnitLogger = tUnitLogger;

    public void Write(LogEvent logEvent)
    {
        var renderedMessage = logEvent.RenderMessage(CultureInfo.InvariantCulture);

        Log(logEvent.Level, renderedMessage, logEvent.Exception);
    }

    private void Log(LogEventLevel level, string message, Exception? exception)
    {
#pragma warning disable CS8604 // Possible null reference argument.
        switch (level)
        {
            case LogEventLevel.Debug:
                _tUnitLogger.LogDebug(message, exception);
                break;
            case LogEventLevel.Information:
                _tUnitLogger.LogInformation(message, exception);
                break;
            case LogEventLevel.Verbose:
                _tUnitLogger.LogTrace(message, exception);
                break;
            case LogEventLevel.Warning:
                _tUnitLogger.LogWarning(message, exception);
                break;
            case LogEventLevel.Error:
                _tUnitLogger.LogError(message, exception);
                break;
            case LogEventLevel.Fatal:
                _tUnitLogger.LogCritical(message, exception);
                break;
            default:
                throw new UnreachableException();
        }
    }
}