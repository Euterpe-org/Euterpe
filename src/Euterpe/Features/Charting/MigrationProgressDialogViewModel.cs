using Euterpe.Models.Migrations;
using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.Features.Charting;

[PerGame]
public sealed partial class MigrationProgressDialogViewModel : ViewModelBase, IDialogContext
{
    [ObservableProperty]
    public partial double Progress { get; set; }

    [ObservableProperty]
    public partial string ProgressLabel { get; set; } = string.Empty;

    public event EventHandler<object?>? RequestClose;

    public void Close() => RequestClose?.Invoke(this, EventArgs.Empty);

    public void Reset()
    {
        Progress = 0;
        ProgressLabel = string.Empty;
    }

    public void Report(MigrationProgress progress)
    {
        Progress = progress.Percentage;
        ProgressLabel = $"{progress.Completed}/{progress.Total}";
    }
}
