using System.Collections.ObjectModel;

namespace Euterpe.Models.Games;

public sealed partial class WizardState : ObservableObject
{
    public ObservableCollection<WizardStepState> Steps { get; } = [];

    [ObservableProperty]
    public partial bool IsRunning { get; set; }

    [ObservableProperty]
    public partial bool IsExecutionFinished { get; set; }

    public bool AllSucceeded => Steps.Count > 0 && Steps.All(s => s.Status is WizardStepStatus.Succeeded);

    public void Reset()
    {
        Steps.Clear();
        IsRunning = false;
        IsExecutionFinished = false;
    }
}