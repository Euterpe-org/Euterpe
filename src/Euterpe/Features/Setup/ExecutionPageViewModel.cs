namespace Euterpe.Features.Setup;

public sealed partial class ExecutionPageViewModel : SetupPageViewModelBase
{
    private Dictionary<SetupOptionKinds, ISetupStep> StepMap => field ??= SetupSteps.ToDictionary(s => s.Kinds);

    public override LocalizedString Title => Setup_Title_SettingUp;

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
        var selected = GameConfig.SetupOptions.Where(o => o.IsSelected).ToArray();
        Logger.ZLogInformation($"Starting setup execution with {selected.Length} step(s): {string.Join(", ", selected.Select(o => o.Kinds))}");

        foreach (var option in selected)
        {
            State.Steps.Add(new SetupStepState
            {
                Kinds = option.Kinds,
                DisplayName = option.DisplayName
            });
        }

        await RunAllAsync().ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task RetryAsync(SetupStepState step)
    {
        Logger.ZLogInformation($"User retrying step '{step.Kinds}'");

        State.Stage = SetupExecutionStage.Running;
        try
        {
            await RunStepAsync(step).ConfigureAwait(true);
        }
        finally
        {
            State.Stage = SetupExecutionStage.Finished;
        }
    }

    private async Task RunAllAsync()
    {
        State.Stage = SetupExecutionStage.Running;
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
            State.Stage = SetupExecutionStage.Finished;
        }
    }

    private async Task RunStepAsync(SetupStepState step)
    {
        step.Status = SetupStepStatus.Running;
        step.ErrorMessage = null;
        step.Message = null;

        var progress = new Progress<string>(msg => step.Message = msg);

        Logger.ZLogInformation($"Running setup step '{step.Kinds}'");
        try
        {
            await StepMap[step.Kinds].ExecuteAsync(progress).ConfigureAwait(true);
            step.Status = SetupStepStatus.Succeeded;
            Logger.ZLogInformation($"Completed setup step '{step.Kinds}'");
        }
        catch (Exception ex)
        {
            step.Status = SetupStepStatus.Failed;
            step.ErrorMessage = Setup_Step_Failed;
            Logger.ZLogError(ex, $"Setup step '{step.Kinds}' failed");
        }
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required IEnumerable<ISetupStep> SetupSteps { get; init; }
    public required ILogger<ExecutionPageViewModel> Logger { get; init; }

    #endregion Injections
}