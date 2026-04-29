using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.ViewModels.Components;

public sealed partial class WizardDialogViewModel : ViewModelBase, IDialogContext
{
    private static readonly Dictionary<WizardIdentity, WizardOptionKinds> Presets = new()
    {
        [WizardIdentity.Player] = WizardOptionKinds.MelonLoader
                                  | WizardOptionKinds.EssentialMods
                                  | WizardOptionKinds.UninstallConflicts,
        [WizardIdentity.Charter] = WizardOptionKinds.MelonLoader
                                   | WizardOptionKinds.EssentialMods
                                   | WizardOptionKinds.UninstallConflicts
                                   | WizardOptionKinds.ChartingTool,
        [WizardIdentity.Modder] = WizardOptionKinds.MelonLoader
                                  | WizardOptionKinds.EssentialMods
                                  | WizardOptionKinds.UninstallConflicts
                                  | WizardOptionKinds.DotNetSdk
                                  | WizardOptionKinds.ModTemplate
                                  | WizardOptionKinds.EnvVariable
    };

    private bool _applyingPreset;
    private Dictionary<WizardOptionKinds, IWizardStep> _stepMap = null!;

    public IReadOnlyList<WizardOption> Options { get; } =
    [
        new(WizardOptionKinds.MelonLoader, Wizard_Task_MelonLoader, Wizard_Task_MelonLoader_Description) { IsSelected = true, IsRequired = true },
        new(WizardOptionKinds.EssentialMods, Wizard_Task_EssentialMods, Wizard_Task_EssentialMods_Description) { IsSelected = true, IsRequired = true },
        new(WizardOptionKinds.UninstallConflicts, Wizard_Task_UninstallConflicts, Wizard_Task_UninstallConflicts_Description) { IsSelected = true, IsRequired = true },
        new(WizardOptionKinds.ChartingTool, Wizard_Task_ChartingTool, Wizard_Task_ChartingTool_Description),
        new(WizardOptionKinds.DotNetSdk, Wizard_Task_DotNetSdk, Wizard_Task_DotNetSdk_Description),
        new(WizardOptionKinds.ModTemplate, Wizard_Task_ModTemplate, Wizard_Task_ModTemplate_Description),
        new(WizardOptionKinds.EnvVariable, Wizard_Task_EnvVariable, Wizard_Task_EnvVariable_Description)
    ];

    public static IReadOnlyList<WizardRole> Roles { get; } =
    [
        new(WizardIdentity.Player, "UserGroup", Wizard_Role_Player, Wizard_Role_Player_Description, "#2563EB"),
        new(WizardIdentity.Charter, "Language", Wizard_Role_Charter, Wizard_Role_Charter_Description, "#7B2CBF"),
        new(WizardIdentity.Modder, "Code", Wizard_Role_Modder, Wizard_Role_Modder_Description, "#047857"),
        new(WizardIdentity.Custom, "Setting", Wizard_Role_Custom, Wizard_Role_Custom_Description, "#B45309")
    ];

    [ObservableProperty]
    public partial bool IsSettingUp { get; set; }

    [ObservableProperty]
    public partial double Progress { get; set; }

    [ObservableProperty]
    public partial string ProgressLabel { get; set; } = string.Empty;

    public WizardRole SelectedRole
    {
        get => Roles.First(r => r.Identity == ComputeIdentity());
        set => ApplyPreset(value.Identity);
    }

    public WizardDialogViewModel()
    {
        Options.Select(t => t.ObservePropertyChanged(x => x.IsSelected))
            .Merge()
            .Where(this, (_, self) => !self._applyingPreset)
            .Subscribe(this, (_, self) => self.OnPropertyChanged(nameof(SelectedRole)));
    }

    public void Close() => RequestClose?.Invoke(this, EventArgs.Empty);

    public event EventHandler<object?>? RequestClose;

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        _stepMap = WizardSteps.ToDictionary(s => s.Kinds);

        Logger.ZLogInformation($"{nameof(WizardDialogViewModel)} Initialized");
    }

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        var options = Options.Where(t => t.IsSelected).ToArray();
        IsSettingUp = true;

        var failed = new List<WizardOption>();

        for (var i = 0; i < options.Length; i++)
        {
            if (!await RunStepAsync(options[i], i + 1, options.Length).ConfigureAwait(true))
            {
                failed.Add(options[i]);
            }
        }

        if (failed.Count is 0)
        {
            Config.SetupCompleted = true;
        }
        else
        {
            Logger.ZLogWarning($"Wizard setup incomplete, {failed.Count} step(s) failed: {string.Join(", ", failed.Select(f => f.Kinds))}");
        }

        Close();
    }

    private async Task<bool> RunStepAsync(WizardOption option, int index, int total)
    {
        ProgressLabel = $"{option.DisplayName} ({index}/{total})";
        Progress = (double)(index - 1) / total * 100;

        Logger.ZLogInformation($"Running wizard step '{option.Kinds}'");
        try
        {
            var step = _stepMap[option.Kinds];
            await step.ExecuteAsync().ConfigureAwait(true);
            Logger.ZLogInformation($"Completed wizard step '{option.Kinds}'");
            Progress = (double)index / total * 100;
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Wizard step '{option.Kinds}' failed");
            Progress = (double)index / total * 100;
            return false;
        }
    }

    private void ApplyPreset(WizardIdentity identity)
    {
        if (identity is WizardIdentity.Custom)
        {
            return;
        }

        _applyingPreset = true;
        try
        {
            var preset = Presets[identity];
            foreach (var option in Options)
            {
                option.IsSelected = preset.HasFlag(option.Kinds);
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
        var selected = Options.Where(t => t.IsSelected)
            .Select(x => x.Kinds)
            .Aggregate(WizardOptionKinds.None, (mask, k) => mask | k);

        foreach (var (identity, preset) in Presets)
        {
            if (preset == selected)
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