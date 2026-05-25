using ObservableCollections;

namespace Euterpe.Features.Logging;

[Route("/logging/app", DisplayName = Panel_Logging_AppLog, Order = 0)]
public sealed class AppLogPanelViewModel : ViewModelBase
{
    public INotifyCollectionChangedSynchronizedViewList<LogMessage> LogMessagesView => LiveLogService.LogMessagesView;

    public required ILogger<AppLogPanelViewModel> Logger { get; init; }

    public required LiveLogService LiveLogService { get; init; }
}