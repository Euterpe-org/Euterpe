using System.Net;
using System.Reflection;
using Euterpe.Core.Http.Handlers;
using Euterpe.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Euterpe.Tests;

[Category("ServerErrorHandlerTests")]
[NotInParallel("ServerErrorHandlerStaticState")]
[TestSubject(typeof(ServerErrorHandler))]
public sealed class ServerErrorHandlerTest
{
    private static void ResetDebounce()
    {
        var field = typeof(ServerErrorHandler).GetField(
            "LastNotifiedTimestamp",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        field.SetValue(null, 0L);
    }

    private static IServiceProvider BuildServices(INotificationService notificationService)
    {
        var services = new ServiceCollection();
        services.AddSingleton(notificationService);
        return services.BuildServiceProvider();
    }

    [Before(Test)]
    public void Setup() => ResetDebounce();

    [Test]
    public async Task SendAsync_SuccessResponse_DoesNotNotifyOrLog()
    {
        var notification = INotificationService.Mock();
        var logger = Mock.Logger<ServerErrorHandler>();
        var inner = new FakeHttpMessageHandler();
        using var handler = new ServerErrorHandler(BuildServices(notification), logger) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/");

        using var assertions = Assert.Multiple();
        await Assert.That(logger.Entries).IsEmpty();
        notification.Warning(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task SendAsync_4xxResponse_DoesNotNotify()
    {
        var notification = INotificationService.Mock();
        var logger = Mock.Logger<ServerErrorHandler>();
        var inner = new FakeHttpMessageHandler(HttpStatusCode.NotFound);
        using var handler = new ServerErrorHandler(BuildServices(notification), logger) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/");

        notification.Warning(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task SendAsync_5xxResponse_LogsWarningAndNotifies()
    {
        var notification = INotificationService.Mock();
        var logger = Mock.Logger<ServerErrorHandler>();
        var inner = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError);
        using var handler = new ServerErrorHandler(BuildServices(notification), logger) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/api/x");

        using var assertions = Assert.Multiple();
        await Assert.That(logger.Entries).HasSingleItem();
        await Assert.That(logger.Entries[0].LogLevel).IsEqualTo(LogLevel.Warning);
        await Assert.That(logger.Entries[0].Message).Contains("500");
        await Assert.That(logger.Entries[0].Message).Contains("https://example.test/api/x");
        notification.Warning(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Once);
    }

    [Test]
    public async Task SendAsync_ConsecutiveServerErrors_DebouncesNotification()
    {
        var notification = INotificationService.Mock();
        var logger = Mock.Logger<ServerErrorHandler>();
        var inner = new FakeHttpMessageHandler(HttpStatusCode.BadGateway);
        using var handler = new ServerErrorHandler(BuildServices(notification), logger) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/");
        await client.GetAsync("https://example.test/");
        await client.GetAsync("https://example.test/");

        notification.Warning(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Once);
    }
}
