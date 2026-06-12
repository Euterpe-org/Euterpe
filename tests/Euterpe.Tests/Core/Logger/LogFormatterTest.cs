using System.Text;
using Euterpe.Core.Logger;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Euterpe.Tests;

[Category("LogFormatterTests")]
public sealed class LogFormatterTest
{
    private const string Category = "TestCategory";

    private static string Capture(Action<ILogger> log, Func<IZLoggerFormatter> formatterFactory)
    {
        var stream = new MemoryStream();
        using (var factory = LoggerFactory.Create(builder =>
               {
                   builder.SetMinimumLevel(LogLevel.Trace);
                   builder.AddZLoggerStream(stream, options => options.UseFormatter(formatterFactory));
               }))
        {
            log(factory.CreateLogger(Category));
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    [Test]
    [Arguments(LogLevel.Trace, "TRC")]
    [Arguments(LogLevel.Debug, "DBG")]
    [Arguments(LogLevel.Information, "INF")]
    [Arguments(LogLevel.Warning, "WRN")]
    [Arguments(LogLevel.Error, "ERR")]
    [Arguments(LogLevel.Critical, "CRT")]
    public async Task ConsoleFormatter_LevelAbbreviationAndCategory(LogLevel level, string abbrev)
    {
        var output = Capture(
            logger => logger.ZLog(level, $"hello"),
            () => new LogConsoleFormatter());

        using var _ = Assert.Multiple();
        await Assert.That(output).Contains($"[{abbrev}]");
        await Assert.That(output).Contains($"({Category})");
        await Assert.That(output).Contains("hello");
    }

    [Test]
    public async Task ConsoleFormatter_WithException_AppendsExceptionDetails()
    {
        var ex = new InvalidOperationException("boom");
        var output = Capture(
            logger => logger.ZLogError(ex, $"failed"),
            () => new LogConsoleFormatter());

        using var _ = Assert.Multiple();
        await Assert.That(output).Contains("[ERR]");
        await Assert.That(output).Contains("failed");
        await Assert.That(output).Contains("InvalidOperationException");
        await Assert.That(output).Contains("boom");
    }

    [Test]
    public async Task FileFormatter_IncludesTimestampLevelCategoryAndMessage()
    {
        var output = Capture(
            logger => logger.ZLogInformation($"file message"),
            () => new LogFileFormatter());

        using var _ = Assert.Multiple();
        await Assert.That(output).Contains("[Information]");
        await Assert.That(output).Contains($"({Category})");
        await Assert.That(output).Contains("file message");
    }

    [Test]
    public async Task FileFormatter_WithException_AppendsExceptionToString()
    {
        var ex = new InvalidOperationException("file boom");
        var output = Capture(
            logger => logger.ZLogError(ex, $"file failed"),
            () => new LogFileFormatter());

        using var _ = Assert.Multiple();
        await Assert.That(output).Contains("[Error]");
        await Assert.That(output).Contains("file failed");
        await Assert.That(output).Contains("file boom");
    }
}
