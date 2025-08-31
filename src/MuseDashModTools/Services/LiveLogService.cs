using ObservableCollections;

namespace MuseDashModTools.Services;

public sealed class LiveLogService
{
    private readonly ObservableFixedSizeRingBuffer<LogMessage> _logMessages = new(50);
    public readonly INotifyCollectionChangedSynchronizedViewList<LogMessage> LogMessagesView;

    public LiveLogService(InMemoryObservableLogProcessor processor)
    {
        processor.MessageReceived += str => _logMessages.AddLast(str);
        var view = _logMessages.CreateView(x => x);
        view.AttachFilter(x => !(x.Message.Contains("Navigating") || x.Message.Contains("Initialized")));
        LogMessagesView = view.ToNotifyCollectionChanged();
    }

    public sealed class LogMessage
    {
        public string Message { get; }

        public LogLevel LogLevel { get; }

        private LogMessage(string message)
        {
            Message = message;

            var span = message.AsSpan()[11..];
            var levelSpan = span[1..span.IndexOf(']')];

            LogLevel = levelSpan switch
            {
                "Trace" => LogLevel.Trace,
                "Debug" => LogLevel.Debug,
                "Information" => LogLevel.Information,
                "Warning" => LogLevel.Warning,
                "Error" => LogLevel.Error,
                "Critical" => LogLevel.Critical,
                _ => throw new UnreachableException()
            };
        }

        public static implicit operator LogMessage(string message) => new(message);
    }
}