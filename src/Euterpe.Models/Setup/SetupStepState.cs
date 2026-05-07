using System.Diagnostics;

namespace Euterpe.Models.Setup;

public sealed partial class SetupStepState : ObservableObject
{
    public required SetupOptionKinds Kinds { get; init; }

    public required LocalizedString DisplayName { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRetry))]
    [NotifyPropertyChangedFor(nameof(StatusDisplay))]
    public partial SetupStepStatus Status { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? Message { get; set; }

    public bool CanRetry => Status is SetupStepStatus.Failed;

    public LocalizedString StatusDisplay => Status switch
    {
        SetupStepStatus.Pending => Wizard_StepStatus_Pending,
        SetupStepStatus.Running => Wizard_StepStatus_Running,
        SetupStepStatus.Succeeded => Wizard_StepStatus_Succeeded,
        SetupStepStatus.Failed => Wizard_StepStatus_Failed,
        _ => throw new UnreachableException()
    };
}