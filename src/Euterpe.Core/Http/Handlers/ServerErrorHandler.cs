namespace Euterpe.Core.Http.Handlers;

internal sealed class ServerErrorHandler(IServiceProvider services, ILogger<ServerErrorHandler> logger) : DelegatingHandler
{
    private static readonly TimeSpan DebounceWindow = TimeSpan.FromSeconds(30);
    private static long LastNotifiedTimestamp;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if ((int)response.StatusCode >= 500)
        {
            NotifyIfNeeded(request, response);
        }

        return response;
    }

    private void NotifyIfNeeded(HttpRequestMessage request, HttpResponseMessage response)
    {
        var now = Stopwatch.GetTimestamp();
        var last = Interlocked.Read(ref LastNotifiedTimestamp);
        if (last != 0 && Stopwatch.GetElapsedTime(last, now) < DebounceWindow)
        {
            return;
        }

        if (Interlocked.CompareExchange(ref LastNotifiedTimestamp, now, last) != last)
        {
            return;
        }

        logger.ZLogWarning($"Server error {(int)response.StatusCode} on {request.Method} {request.RequestUri}");
        services.GetRequiredService<INotificationService>().Warning(Notification_Content_Server_Error);
    }
}