using System.Diagnostics;

namespace Euterpe.Models.Games;

public sealed partial class WizardStepState(WizardOption option) : ObservableObject
{
    public WizardOption Option { get; } = option;

    public WizardOptionKinds Kinds => Option.Kinds;

    public LocalizedString DisplayName => Option.DisplayName;

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