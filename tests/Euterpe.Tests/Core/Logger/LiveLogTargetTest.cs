using Euterpe.Core.Logger;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;
using NLog;
using NLog.Config;

namespace Euterpe.Tests.Core.Logger;

[Category("LiveLogTargetTests")]
[TestSubject(typeof(LiveLogTarget))]
public sealed class LiveLogTargetTest
{
    private static List<LogMessage> RunWith(string category, Action<NLog.Logger> log)
    {
        var target = new LiveLogTarget();
        var captured = new List<LogMessage>();
        target.OnLogMessageReceived += message => captured.Add(message);

        var logFactory = new LogFactory();
        var configuration = new LoggingConfiguration(logFactory);
        configuration.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, target);
        logFactory.Configuration = configuration;
        log(logFactory.GetLogger(category));
        logFactory.Shutdown();

        return captured;
    }

    [Test]
    public async Task Write_NormalCategory_RaisesLogMessageEvent()
    {
        var messages = RunWith("Euterpe.Tests", logger => logger.Info("hello"));

        using var _ = Assert.Multiple();
        await Assert.That(messages).HasSingleItem();
        await Assert.That(messages[0].Message).IsEqualTo("hello");
        await Assert.That(messages[0].LogLevel).IsEqualTo(MicrosoftLogLevel.Information);
        await Assert.That(messages[0].Category).IsEqualTo("Euterpe.Tests");
    }

    [Test]
    public async Task Write_NavigationServiceCategory_FiltersOut()
    {
        var messages = RunWith("Euterpe.Services.NavigationService", logger => logger.Info("navigation event"));

        await Assert.That(messages).IsEmpty();
    }

    [Test]
    public async Task Write_MessageContainsInitialized_FiltersOut()
    {
        var messages = RunWith("Euterpe.Tests", logger => logger.Info("Module Initialized"));

        await Assert.That(messages).IsEmpty();
    }

    [Test]
    public async Task Write_FatalLevel_MapsToCritical()
    {
        var messages = RunWith("Euterpe.Tests", logger => logger.Fatal("failed"));

        await Assert.That(messages.Single().LogLevel).IsEqualTo(MicrosoftLogLevel.Critical);
    }
}
