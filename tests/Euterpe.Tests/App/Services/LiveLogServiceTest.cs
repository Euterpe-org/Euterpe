using System.Collections;
using Euterpe.Core.Logger;
using Euterpe.Services;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Euterpe.Tests.App.Services;

[Category("LiveLogServiceTests")]
[TestSubject(typeof(LiveLogService))]
public sealed class LiveLogServiceTest
{
    private static List<LogMessage> MaterializeView(IEnumerable view)
    {
        var list = new List<LogMessage>();
        foreach (LogMessage item in view)
        {
            list.Add(item);
        }

        return list;
    }

    private static (LiveLogService service, LiveLogProcessor processor, ILoggerFactory factory) CreateWiredService()
    {
        var processor = new LiveLogProcessor();
        var service = new LiveLogService(processor);
        var factory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddZLoggerLogProcessor(_ => processor);
        });
        return (service, processor, factory);
    }

    [Test]
    public async Task Ctor_NoMessages_ViewIsEmpty()
    {
        var processor = new LiveLogProcessor();
        var service = new LiveLogService(processor);

        await Assert.That(service.LogMessagesView).IsEmpty();
    }

    [Test]
    public async Task ProcessorEvent_AppendsMessageToView()
    {
        var (service, _, factory) = CreateWiredService();
        using (factory)
        {
            factory.CreateLogger("Euterpe.Tests").ZLogInformation($"hello world");
        }

        var view = MaterializeView(service.LogMessagesView);
        using var _ = Assert.Multiple();
        await Assert.That(view).HasSingleItem();
        await Assert.That(view[0].Message).Contains("hello world");
        await Assert.That(view[0].LogLevel).IsEqualTo(LogLevel.Information);
    }

    [Test]
    public async Task ProcessorEvent_PreservesAppendOrder()
    {
        var (service, _, factory) = CreateWiredService();
        using (factory)
        {
            var logger = factory.CreateLogger("Euterpe.Tests");
            logger.ZLogInformation($"first");
            logger.ZLogInformation($"second");
            logger.ZLogInformation($"third");
        }

        var view = MaterializeView(service.LogMessagesView);
        using var _ = Assert.Multiple();
        await Assert.That(view).Count().IsEqualTo(3);
        await Assert.That(view[0].Message).Contains("first");
        await Assert.That(view[1].Message).Contains("second");
        await Assert.That(view[2].Message).Contains("third");
    }

    [Test]
    public async Task RingBuffer_DropsOldestWhenExceedingCapacity()
    {
        var (service, _, factory) = CreateWiredService();
        using (factory)
        {
            var logger = factory.CreateLogger("Euterpe.Tests");
            for (var i = 0; i < 55; i++)
            {
                logger.ZLogInformation($"msg-{i}");
            }
        }

        var view = MaterializeView(service.LogMessagesView);
        using var _ = Assert.Multiple();
        await Assert.That(view).Count().IsEqualTo(50);
        await Assert.That(view[0].Message).Contains("msg-5");
        await Assert.That(view[49].Message).Contains("msg-54");
    }

    [Test]
    public async Task ProcessorEvent_FilteredCategory_DoesNotAppendToView()
    {
        var (service, _, factory) = CreateWiredService();
        using (factory)
        {
            factory.CreateLogger("Euterpe.Services.NavigationService").ZLogInformation($"routed");
        }

        await Assert.That(service.LogMessagesView).IsEmpty();
    }
}
