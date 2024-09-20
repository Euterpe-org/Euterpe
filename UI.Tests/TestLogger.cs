using Serilog;
using Serilog.Events;

namespace MuseDashModToolsUI.Tests;

public sealed class TestLogger(ITestOutputHelper testOutputHelper) : ILogger
{
    public void Write(LogEvent logEvent)
    {
        testOutputHelper.WriteLine(logEvent.MessageTemplate.Render(logEvent.Properties, CultureInfo.InvariantCulture));
        if (logEvent.Exception is not null)
        {
            testOutputHelper.WriteLine(logEvent.Exception.ToString());
        }
    }

    public void DebugOutput(string output) => testOutputHelper.WriteLine(output);
}