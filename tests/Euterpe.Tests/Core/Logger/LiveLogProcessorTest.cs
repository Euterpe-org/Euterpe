using Euterpe.Core.Logger;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Euterpe.Tests;

[Category("LiveLogProcessorTests")]
[TestSubject(typeof(LiveLogProcessor))]
public sealed class LiveLogProcessorTest
{
    private static List<LogMessage> RunWith(Action<ILoggerFactory> log)
    {
        var processor = new LiveLogProcessor();
        var captured = new List<LogMessage>();
        processor.OnLogMessageReceived += msg => captured.Add(msg);

        using (var factory = LoggerFactory.Create(builder =>
               {
                   builder.SetMinimumLevel(LogLevel.Trace);
                   builder.AddZLoggerLogProcessor(_ => processor);
               }))
        {
            log(factory);
        }

        return captured;
    }

    [Test]
    public async Task Post_NormalCategory_RaisesLogMessageEvent()
    {
        var messages = RunWith(factory =>
            factory.CreateLogger("Euterpe.Tests").ZLogInformation($"hello"));

        using var _ = Assert.Multiple();
        await Assert.That(messages).HasSingleItem();
        await Assert.That(messages[0].Message).Contains("hello");
        await Assert.That(messages[0].LogLevel).IsEqualTo(LogLevel.Information);
        await Assert.That(messages[0].Category.Name).IsEqualTo("Euterpe.Tests");
    }

    [Test]
    public async Task Post_NavigationServiceCategory_FiltersOut()
    {
        var messages = RunWith(factory =>
            factory.CreateLogger("Euterpe.Services.NavigationService").ZLogInformation($"navigation event"));

        await Assert.That(messages).IsEmpty();
    }

    [Test]
    public async Task Post_MessageContainsInitialized_FiltersOut()
    {
        var messages = RunWith(factory =>
            factory.CreateLogger("Euterpe.Tests").ZLogInformation($"Module Initialized"));

        await Assert.That(messages).IsEmpty();
    }

    [Test]
    public async Task DisposeAsync_DefaultValueTask_DoesNotThrow()
    {
        var processor = new LiveLogProcessor();
        await processor.DisposeAsync();
    }
}
