namespace Euterpe.ViewModels.Components.Wizard;

public sealed partial class ExecutionPageViewModel : WizardPageViewModelBase
{
    private Dictionary<WizardOptionKinds, IWizardStep> StepMap => field ??= WizardSteps.ToDictionary(s => s.Kinds);

    public override LocalizedString Title => Wizard_Title_SettingUp;

    public override bool CanGoBack => false;

    public override bool CanGoNext => State.AllSucceeded;

    public override LocalizedString NextButtonText => Button_Confirm;

    [ObservableProperty]
    public partial double Progress { get; set; }

    [ObservableProperty]
    public partial string ProgressLabel { get; set; } = string.Empty;

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        State.ObservePropertyChanged(x => x.Stage)
            .Subscribe(this, (_, self) => self.OnPropertyChanged(nameof(CanGoNext)));

        Logger.ZLogInformation($"{nameof(ExecutionPageViewModel)} Initialized");
    }

    public override async Task OnEnterAsync()
    {
        var selected = GameConfig.WizardOptions.Where(o => o.IsSelected).ToArray();
        Logger.ZLogInformation($"Starting wizard execution with {selected.Length} step(s): {string.Join(", ", selected.Select(o => o.Kinds))}");

        foreach (var option in selected)
        {
            State.Steps.Add(new WizardStepState
            {
                Kinds = option.Kinds,
                DisplayName = option.DisplayName
            });
        }

        await RunAllAsync().ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task RetryAsync(WizardStepState step)
    {
        Logger.ZLogInformation($"User retrying step '{step.Kinds}'");

        State.Stage = WizardExecutionStage.Running;
        try
        {
            await RunStepAsync(step).ConfigureAwait(true);
        }
        finally
        {
            State.Stage = WizardExecutionStage.Finished;
        }
    }

    private async Task RunAllAsync()
    {
        State.Stage = WizardExecutionStage.Running;
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
            State.Stage = WizardExecutionStage.Finished;
        }
    }

    private async Task RunStepAsync(WizardStepState step)
    {
        step.Status = WizardStepStatus.Running;
        step.ErrorMessage = null;
        step.Message = null;

        var progress = new Progress<string>(msg => step.Message = msg);

        Logger.ZLogInformation($"Running wizard step '{step.Kinds}'");
        try
        {
            await StepMap[step.Kinds].ExecuteAsync(progress).ConfigureAwait(true);
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

    #region Injections

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required IEnumerable<IWizardStep> WizardSteps { get; init; }

    [UsedImplicitly]
    public required ILogger<ExecutionPageViewModel> Logger { get; init; }

    #endregion Injections
}