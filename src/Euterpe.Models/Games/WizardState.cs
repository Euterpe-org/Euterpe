using System.Collections.ObjectModel;

namespace Euterpe.Models.Games;

public sealed partial class WizardState : ObservableObject
{
    public ObservableCollection<WizardStepState> Steps { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRunning))]
    [NotifyPropertyChangedFor(nameof(IsFinished))]
    [NotifyPropertyChangedFor(nameof(AllSucceeded))]
    public partial WizardExecutionStage Stage { get; set; }

    public bool IsRunning => Stage is WizardExecutionStage.Running;

    public bool IsFinished => Stage is WizardExecutionStage.Finished;

    public bool AllSucceeded => IsFinished && Steps.All(s => s.Status is WizardStepStatus.Succeeded);

    public void Reset()
    {
        Steps.Clear();
        Stage = WizardExecutionStage.NotStarted;
    }
}