using Serilog.Events;
using Serilog.Formatting;

namespace MuseDashModTools.Core;

public sealed class LogFileFormatter : ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        output.WriteLine($"[{logEvent.Timestamp:HH:mm:ss.fff zzz}] [{logEvent.Level}]");
        output.WriteLine(logEvent.MessageTemplate.Render(logEvent.Properties, CultureInfo.InvariantCulture));
        if (logEvent.Exception is not null)
        {
            output.WriteLine(logEvent.Exception.ToString());
        }

        output.WriteLine("------------------------------");
        output.Write(output.NewLine);
    }
}