namespace Euterpe.Models.Games;

public sealed partial class WizardStepState(WizardOption option) : ObservableObject
{
    public WizardOption Option { get; } = option;

    public WizardOptionKinds Kinds => Option.Kinds;

    public LocalizedString DisplayName => Option.DisplayName;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRetry))]
    public partial WizardStepStatus Status { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    public bool CanRetry => Status is WizardStepStatus.Failed;
}