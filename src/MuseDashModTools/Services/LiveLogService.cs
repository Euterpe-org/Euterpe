using System.Collections.ObjectModel;

namespace MuseDashModTools.Services;

public sealed class LiveLogService
{
    public ObservableCollection<string> LogContents { get; } = [];

    public LiveLogService(InMemoryObservableLogProcessor processor)
    {
        processor.MessageReceived += s => LogContents.Add(s);
    }
}