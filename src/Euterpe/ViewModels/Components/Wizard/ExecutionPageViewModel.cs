namespace Euterpe.ViewModels.Components.Wizard;

public sealed partial class ExecutionPageViewModel : WizardPageViewModelBase
{
    private Dictionary<WizardOptionKinds, IWizardStep> _stepMap = null!;

    public override LocalizedString Title => Wizard_Title_SettingUp;

    public override bool CanGoBack => false;

    public override bool CanGoNext => State.IsExecutionFinished;

    public override LocalizedString NextButtonText => Button_Confirm;

    [ObservableProperty]
    public partial double Progress { get; set; }

    [ObservableProperty]
    public partial string ProgressLabel { get; set; } = string.Empty;

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        _stepMap = WizardSteps.ToDictionary(s => s.Kinds);

        State.ObservePropertyChanged(x => x.IsExecutionFinished)
            .Subscribe(this, (_, self) => self.OnPropertyChanged(nameof(CanGoNext)));

        Logger.ZLogInformation($"{nameof(ExecutionPageViewModel)} Initialized");
    }

    public override async Task OnEnterAsync()
    {
        if (State.Steps.Count > 0)
        {
            return;
        }

        var selected = GameConfig.WizardOptions.Where(o => o.IsSelected).ToArray();
        Logger.ZLogInformation($"Starting wizard execution with {selected.Length} step(s): {string.Join(", ", selected.Select(o => o.Kinds))}");

        foreach (var option in selected)
        {
            State.Steps.Add(new WizardStepState(option));
        }

        await RunAllAsync().ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task RetryAsync(WizardStepState step)
    {
        if (State.IsRunning || step.Status is WizardStepStatus.Running or WizardStepStatus.Succeeded)
        {
            return;
        }

        Logger.ZLogInformation($"User retrying step '{step.Kinds}'");

        State.IsRunning = true;
        State.IsExecutionFinished = false;
        try
        {
            await RunStepAsync(step).ConfigureAwait(true);
        }
        finally
        {
            State.IsRunning = false;
            UpdateFinishedFlag();
        }
    }

    private async Task RunAllAsync()
    {
        State.IsRunning = true;
        State.IsExecutionFinished = false;
        try
        {
            for (var i = 0; i < State.Steps.Count; i++)
            {
                var step = State.Steps[i];
                ProgressLabel = $"{step.DisplayName} ({i + 1}/{State.Steps.Count})";
                Progress = (double)i / State.Steps.Count * 100;

                await RunStepAsync(step).ConfigureAwait(true);

                Progress = (double)(i + 1) / State.Steps.Count * 100;
            }
        }
        finally
        {
            State.IsRunning = false;
            UpdateFinishedFlag();
        }
    }

    private async Task RunStepAsync(WizardStepState step)
    {
        step.Status = WizardStepStatus.Running;
        step.ErrorMessage = null;

        Logger.ZLogInformation($"Running wizard step '{step.Kinds}'");
        try
        {
            await _stepMap[step.Kinds].ExecuteAsync().ConfigureAwait(true);
            step.Status = WizardStepStatus.Succeeded;
            Logger.ZLogInformation($"Completed wizard step '{step.Kinds}'");
        }
        catch (Exception ex)
        {
            step.Status = WizardStepStatus.Failed;
            step.ErrorMessage = ex.Message;
            Logger.ZLogError(ex, $"Wizard step '{step.Kinds}' failed");
        }
    }

    private void UpdateFinishedFlag() =>
        State.IsExecutionFinished = State.Steps.All(s => s.Status is WizardStepStatus.Succeeded or WizardStepStatus.Failed);

    #region Injections

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required IEnumerable<IWizardStep> WizardSteps { get; init; }

    [UsedImplicitly]
    public required ILogger<ExecutionPageViewModel> Logger { get; init; }

    #endregion Injections
}