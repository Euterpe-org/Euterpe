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
        SetupStepStatus.Pending => Setup_StepStatus_Pending,
        SetupStepStatus.Running => Setup_StepStatus_Running,
        SetupStepStatus.Succeeded => Setup_StepStatus_Succeeded,
        SetupStepStatus.Failed => Setup_StepStatus_Failed,
        _ => throw new UnreachableException()
    };
}