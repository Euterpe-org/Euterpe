using Microsoft.Extensions.Logging;
using TUnitLogLevel = TUnit.Core.Logging.LogLevel;

namespace MuseDashModTools.Tests;

public sealed class TestLogger<T> : ILogger<T>, IDisposable
{
    public void Dispose()
    {
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        TestContext.Current?.GetDefaultLogger().Log(MapLogLevel(logLevel), state, exception, formatter);
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => this;

    private static TUnitLogLevel MapLogLevel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => TUnitLogLevel.Trace,
            LogLevel.Debug => TUnitLogLevel.Debug,
            LogLevel.Information => TUnitLogLevel.Information,
            LogLevel.Warning => TUnitLogLevel.Warning,
            LogLevel.Error => TUnitLogLevel.Error,
            LogLevel.Critical => TUnitLogLevel.Critical,
            LogLevel.None => TUnitLogLevel.None,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };
    }
}