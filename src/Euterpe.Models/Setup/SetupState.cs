using System.Collections.ObjectModel;

namespace Euterpe.Models.Setup;

public sealed partial class SetupState : ObservableObject
{
    public ObservableCollection<SetupStepState> Steps { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRunning))]
    [NotifyPropertyChangedFor(nameof(AllSucceeded))]
    public partial SetupExecutionStage Stage { get; set; }

    public bool IsRunning => Stage is SetupExecutionStage.Running;

    public bool AllSucceeded => Stage is SetupExecutionStage.Finished && Steps.All(s => s.Status is SetupStepStatus.Succeeded);

    public void Reset()
    {
        Steps.Clear();
        Stage = SetupExecutionStage.NotStarted;
    }
}
