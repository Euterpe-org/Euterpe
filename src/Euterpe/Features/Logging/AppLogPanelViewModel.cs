using ObservableCollections;

namespace Euterpe.Features.Logging;

public sealed class AppLogPanelViewModel : ViewModelBase
{
    public INotifyCollectionChangedSynchronizedViewList<LogMessage> LogMessagesView => LiveLogService.LogMessagesView;

    [UsedImplicitly]
    public required ILogger<AppLogPanelViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required LiveLogService LiveLogService { get; init; }
}