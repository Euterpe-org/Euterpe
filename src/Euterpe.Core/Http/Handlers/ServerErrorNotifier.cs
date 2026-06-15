namespace Euterpe.Core.Http.Handlers;

internal sealed class ServerErrorNotifier(IServiceProvider services, ILogger<ServerErrorNotifier> logger)
{
    private static readonly TimeSpan DebounceWindow = TimeSpan.FromSeconds(30);
    private long _lastNotifiedTimestamp;

    public void NotifyIfNeeded(HttpRequestMessage request, HttpResponseMessage response)
    {
        var now = Stopwatch.GetTimestamp();
        var last = Interlocked.Read(ref _lastNotifiedTimestamp);
        if (last != 0 && Stopwatch.GetElapsedTime(last, now) < DebounceWindow)
        {
            return;
        }

        if (Interlocked.CompareExchange(ref _lastNotifiedTimestamp, now, last) != last)
        {
            return;
        }

        logger.ZLogWarning($"Server error {(int)response.StatusCode} on {request.Method} {request.RequestUri}");
        services.GetRequiredService<INotificationService>().Warning(Notification_Content_Server_Error);
    }
}
