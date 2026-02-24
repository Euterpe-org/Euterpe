namespace Euterpe.Core.Logger;

public sealed class LiveLogProcessor : IAsyncLogProcessor
{
    public void Post(IZLoggerEntry log)
    {
        var category = log.LogInfo.Category;

        if (category.Name is "Euterpe.Services.NavigationService")
        {
            log.Return();
            return;
        }

        var logMessage = new LogMessage(
            log.LogInfo.Timestamp.Local,
            log.LogInfo.LogLevel,
            category,
            log.ToString()
        );

        log.Return();

        if (logMessage.Message.Contains("Initialized"))
            return;

        OnLogMessageReceived?.Invoke(logMessage);
    }

    public ValueTask DisposeAsync() => default;

    public event Action<LogMessage>? OnLogMessageReceived;
}