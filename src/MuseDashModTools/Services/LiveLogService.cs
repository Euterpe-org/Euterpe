using ObservableCollections;

namespace MuseDashModTools.Services;

public sealed class LiveLogService
{
    private readonly ObservableFixedSizeRingBuffer<string> _logMessages = new(50);
    public readonly INotifyCollectionChangedSynchronizedViewList<string> LogMessagesView;

    public LiveLogService(InMemoryObservableLogProcessor processor)
    {
        processor.MessageReceived += str => _logMessages.AddLast(str);
        LogMessagesView = _logMessages.ToNotifyCollectionChanged();
    }
}