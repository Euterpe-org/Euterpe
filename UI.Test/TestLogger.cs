using System.Globalization;
using Serilog;
using Serilog.Events;

namespace MuseDashModToolsUI.Test;

public class TestLogger : ILogger
{
    private ITestOutputHelper TestOutputHelper { get; }

    public TestLogger(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    public void Write(LogEvent logEvent)
    {
        TestOutputHelper.WriteLine(logEvent.MessageTemplate.Render(logEvent.Properties, CultureInfo.InvariantCulture));
        if (logEvent.Exception is not null)
            TestOutputHelper.WriteLine(logEvent.Exception.ToString());
    }
}