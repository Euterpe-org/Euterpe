namespace Euterpe.Core.Http.Handlers;

internal sealed class ServerErrorHandler(IServiceProvider services, ILogger<ServerErrorHandler> logger) : DelegatingHandler
{
    private static readonly TimeSpan DebounceWindow = TimeSpan.FromSeconds(30);
    private static long LastNotifiedTicks;

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
        var nowTicks = DateTime.UtcNow.Ticks;
        var lastTicks = Interlocked.Read(ref LastNotifiedTicks);
        if (new TimeSpan(nowTicks - lastTicks) < DebounceWindow)
        {
            return;
        }

        if (Interlocked.CompareExchange(ref LastNotifiedTicks, nowTicks, lastTicks) != lastTicks)
        {
            return;
        }

        logger.ZLogWarning($"Server error {(int)response.StatusCode} on {request.Method} {request.RequestUri}");
        services.GetRequiredService<INotificationService>().Warning(Notification_Content_Server_Error);
    }
}