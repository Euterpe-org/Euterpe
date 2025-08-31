namespace MuseDashModTools.Core.Logger;

public sealed class LiveLogProcessor : IAsyncLogProcessor
{
    public void Post(IZLoggerEntry log)
    {
        var logMessage = new LogMessage(
            log.LogInfo.Timestamp.Local,
            log.LogInfo.LogLevel,
            log.ToString()
        );

        log.Return();

        OnLogMessageReceived?.Invoke(logMessage);
    }

    public ValueTask DisposeAsync() => default;

    public event Action<LogMessage>? OnLogMessageReceived;
}