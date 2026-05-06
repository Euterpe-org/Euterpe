using System.Diagnostics;

namespace Euterpe.Models.Games;

public sealed partial class WizardStepState : ObservableObject
{
    public required WizardOptionKinds Kinds { get; init; }

    public required LocalizedString DisplayName { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRetry))]
    [NotifyPropertyChangedFor(nameof(StatusDisplay))]
    public partial WizardStepStatus Status { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    public bool CanRetry => Status is WizardStepStatus.Failed;

    public LocalizedString StatusDisplay => Status switch
    {
        WizardStepStatus.Pending => Wizard_StepStatus_Pending,
        WizardStepStatus.Running => Wizard_StepStatus_Running,
        WizardStepStatus.Succeeded => Wizard_StepStatus_Succeeded,
        WizardStepStatus.Failed => Wizard_StepStatus_Failed,
        _ => throw new UnreachableException()
    };
}