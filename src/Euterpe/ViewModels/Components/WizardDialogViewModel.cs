using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.ViewModels.Components;

public sealed partial class WizardDialogViewModel : ViewModelBase, IDialogContext
{
    private static readonly Dictionary<WizardIdentity, HashSet<WizardTaskKind>> Presets = new()
    {
        [WizardIdentity.Player] =
        [
            WizardTaskKind.MelonLoader,
            WizardTaskKind.EssentialMods,
            WizardTaskKind.UninstallConflicts
        ],
        [WizardIdentity.Charter] =
        [
            WizardTaskKind.MelonLoader,
            WizardTaskKind.EssentialMods,
            WizardTaskKind.UninstallConflicts,
            WizardTaskKind.ChartingTool
        ],
        [WizardIdentity.Modder] =
        [
            WizardTaskKind.MelonLoader,
            WizardTaskKind.EssentialMods,
            WizardTaskKind.UninstallConflicts,
            WizardTaskKind.DotNetSdk,
            WizardTaskKind.ModTemplate,
            WizardTaskKind.EnvVariable
        ]
    };

    private bool _applyingPreset;
    private Dictionary<WizardTaskKind, IWizardStep> _stepMap = null!;

    public static IReadOnlyList<WizardRole> Roles { get; } =
    [
        new(WizardIdentity.Player, "UserGroup", Wizard_Role_Player, Wizard_Role_Player_Description, "#2563EB"),
        new(WizardIdentity.Charter, "Language", Wizard_Role_Charter, Wizard_Role_Charter_Description, "#7B2CBF"),
        new(WizardIdentity.Modder, "Code", Wizard_Role_Modder, Wizard_Role_Modder_Description, "#047857"),
        new(WizardIdentity.Custom, "Setting", Wizard_Role_Custom, Wizard_Role_Custom_Description, "#B45309")
    ];

    public IReadOnlyList<WizardTask> Tasks { get; } =
    [
        new(WizardTaskKind.MelonLoader, Wizard_Task_MelonLoader, Wizard_Task_MelonLoader_Description) { IsSelected = true },
        new(WizardTaskKind.EssentialMods, Wizard_Task_EssentialMods, Wizard_Task_EssentialMods_Description) { IsSelected = true },
        new(WizardTaskKind.UninstallConflicts, Wizard_Task_UninstallConflicts, Wizard_Task_UninstallConflicts_Description) { IsSelected = true },
        new(WizardTaskKind.ChartingTool, Wizard_Task_ChartingTool, Wizard_Task_ChartingTool_Description),
        new(WizardTaskKind.DotNetSdk, Wizard_Task_DotNetSdk, Wizard_Task_DotNetSdk_Description),
        new(WizardTaskKind.ModTemplate, Wizard_Task_ModTemplate, Wizard_Task_ModTemplate_Description),
        new(WizardTaskKind.EnvVariable, Wizard_Task_EnvVariable, Wizard_Task_EnvVariable_Description)
    ];

    [ObservableProperty]
    public partial bool IsProgressPage { get; set; }

    [ObservableProperty]
    public partial double Progress { get; set; }

    [ObservableProperty]
    public partial string ProgressDescription { get; set; } = string.Empty;

    public WizardRole SelectedRole
    {
        get => Roles.First(r => r.Identity == ComputeIdentity());
        set => ApplyPreset(value.Identity);
    }

    public WizardDialogViewModel()
    {
        Tasks.Select(t => t.ObservePropertyChanged(x => x.IsSelected))
            .Merge()
            .Where(this, (_, self) => !self._applyingPreset)
            .Subscribe(this, (_, self) => self.OnPropertyChanged(nameof(SelectedRole)));
    }

    public void Close() => RequestClose?.Invoke(this, EventArgs.Empty);

    public event EventHandler<object?>? RequestClose;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync().ConfigureAwait(false);

        _stepMap = WizardSteps.ToDictionary(s => s.Kind);

        Logger.ZLogInformation($"{nameof(WizardDialogViewModel)} Initialized");
    }

    [RelayCommand]
    private void SkipWizard()
    {
        Config.SetupCompleted = true;
        Close();
    }

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        var tasks = Tasks.Where(t => t.IsSelected).ToArray();
        IsProgressPage = true;

        for (var i = 0; i < tasks.Length; i++)
        {
            await RunStepAsync(tasks[i], i + 1, tasks.Length).ConfigureAwait(true);
        }

        Config.SetupCompleted = true;
        await SettingService.SaveAsync().ConfigureAwait(true);
        Close();
    }

    private async Task RunStepAsync(WizardTask task, int index, int total)
    {
        ProgressDescription = $"{task.DisplayName} ({index}/{total})";
        Progress = (double)(index - 1) / total * 100;

        if (!_stepMap.TryGetValue(task.Kind, out var step))
        {
            Logger.ZLogWarning($"No IWizardStep registered for '{task.Kind}'");
            return;
        }

        Logger.ZLogInformation($"Running wizard step '{task.Kind}'");
        try
        {
            await step.ExecuteAsync().ConfigureAwait(true);
            Logger.ZLogInformation($"Completed wizard step '{task.Kind}'");
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Wizard step '{task.Kind}' failed");
        }

        Progress = (double)index / total * 100;
    }

    private void ApplyPreset(WizardIdentity identity)
    {
        if (_applyingPreset)
        {
            return;
        }

        _applyingPreset = true;
        try
        {
            if (Presets.TryGetValue(identity, out var preset))
            {
                foreach (var task in Tasks)
                {
                    task.IsSelected = preset.Contains(task.Kind);
                }
            }
        }
        finally
        {
            _applyingPreset = false;
            OnPropertyChanged(nameof(SelectedRole));
        }
    }

    private WizardIdentity ComputeIdentity()
    {
        var selected = Tasks.Where(t => t.IsSelected).Select(t => t.Kind).ToHashSet();
        foreach (var (identity, preset) in Presets)
        {
            if (preset.SetEquals(selected))
            {
                return identity;
            }
        }

        return WizardIdentity.Custom;
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required ISettingService SettingService { get; init; }

    [UsedImplicitly]
    public required IEnumerable<IWizardStep> WizardSteps { get; init; }

    [UsedImplicitly]
    public required ILogger<WizardDialogViewModel> Logger { get; init; }

    #endregion Injections
}