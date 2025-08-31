using ObservableCollections;

namespace MuseDashModTools.ViewModels.Panels.Logging;

public sealed class LogViewerPanelViewModel : ViewModelBase
{
    public INotifyCollectionChangedSynchronizedViewList<LogMessage> LogMessagesView => LiveLogService.LogMessagesView;

    [UsedImplicitly]
    public required ILogger<LogViewerPanelViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required LiveLogService LiveLogService { get; init; }
}