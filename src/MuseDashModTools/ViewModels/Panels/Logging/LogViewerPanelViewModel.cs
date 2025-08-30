using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels.Panels.Logging;

public sealed class LogViewerPanelViewModel : ViewModelBase
{
    public ObservableCollection<string> LogContents => LiveLogService.LogContents;

    [UsedImplicitly]
    public required ILogger<LogViewerPanelViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required LiveLogService LiveLogService { get; init; }
}