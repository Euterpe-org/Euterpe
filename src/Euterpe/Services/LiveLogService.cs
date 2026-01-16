using ObservableCollections;

namespace Euterpe.Services;

public sealed class LiveLogService
{
    private readonly ObservableFixedSizeRingBuffer<LogMessage> _logMessages = new(50);
    public readonly INotifyCollectionChangedSynchronizedViewList<LogMessage> LogMessagesView;

    public LiveLogService(LiveLogProcessor processor)
    {
        processor.OnLogMessageReceived += logMessage => _logMessages.AddLast(logMessage);

        var view = _logMessages.CreateView(x => x);
        view.AttachFilter(x => !(x.Category.Name is "Euterpe.Services.NavigationService" || x.Message.Contains("Initialized")));

        LogMessagesView = view.ToNotifyCollectionChanged();
    }
}