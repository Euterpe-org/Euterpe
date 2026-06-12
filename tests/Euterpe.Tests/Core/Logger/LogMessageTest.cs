using Euterpe.Core.Logger;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Euterpe.Tests;

[Category("LogMessageTests")]
[TestSubject(typeof(LogMessage))]
public sealed class LogMessageTest
{
    [Test]
    [Arguments(LogLevel.Trace, "TRC")]
    [Arguments(LogLevel.Debug, "DBG")]
    [Arguments(LogLevel.Information, "INF")]
    [Arguments(LogLevel.Warning, "WRN")]
    [Arguments(LogLevel.Error, "ERR")]
    [Arguments(LogLevel.Critical, "CRT")]
    [Arguments(LogLevel.None, "NON")]
    public async Task LogLevelAbbreviation_DerivedFromLevel(LogLevel level, string expected)
    {
        var msg = new LogMessage(DateTimeOffset.UtcNow, level, new LogCategory("cat"), "m");
        await Assert.That(msg.LogLevelAbbreviation).IsEqualTo(expected);
    }

    [Test]
    public async Task RecordEquality_ComparesAllFields()
    {
        var ts = new DateTimeOffset(2026, 5, 7, 0, 0, 0, TimeSpan.Zero);
        var category = new LogCategory("cat");

        var a = new LogMessage(ts, LogLevel.Information, category, "msg");
        var b = new LogMessage(ts, LogLevel.Information, category, "msg");
        var c = a with { Message = "different" };

        using var _ = Assert.Multiple();
        await Assert.That(a).IsEqualTo(b);
        await Assert.That(a).IsNotEqualTo(c);
        await Assert.That(a.Timestamp).IsEqualTo(ts);
        await Assert.That(a.LogLevel).IsEqualTo(LogLevel.Information);
        await Assert.That(a.Message).IsEqualTo("msg");
    }
}
