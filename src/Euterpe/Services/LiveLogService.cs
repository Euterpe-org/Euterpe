using ObservableCollections;

namespace Euterpe.Services;

public sealed class LiveLogService
{
    private readonly ObservableFixedSizeRingBuffer<LogMessage> _logMessages = new(50);
    public readonly INotifyCollectionChangedSynchronizedViewList<LogMessage> LogMessagesView;

    public LiveLogService(LiveLogProcessor processor)
    {
        processor.OnLogMessageReceived += logMessage => _logMessages.AddLast(logMessage);

        LogMessagesView = _logMessages.CreateView(x => x).ToNotifyCollectionChanged();
    }
}