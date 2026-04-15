using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.ViewModels.Components;

public sealed partial class WizardDialogViewModel : ViewModelBase, IDialogContext
{
    private static readonly Dictionary<WizardIdentity, HashSet<string>> Presets = new()
    {
        [WizardIdentity.Player] = ["MelonLoader", "EssentialMods", "UninstallConflicts"],
        [WizardIdentity.Charter] = ["MelonLoader", "EssentialMods", "UninstallConflicts", "ChartingTool"],
        [WizardIdentity.Modder] = ["MelonLoader", "EssentialMods", "UninstallConflicts", "DotNetSdk", "ModTemplate", "EnvVariable"]
    };

    private bool _isSyncingRole;

    public static IReadOnlyList<WizardRole> Roles { get; } =
    [
        new(WizardIdentity.Player, "UserGroup", Wizard_Role_Player, Wizard_Role_Player_Description, "#2563EB"),
        new(WizardIdentity.Charter, "Language", Wizard_Role_Charter, Wizard_Role_Charter_Description, "#7B2CBF"),
        new(WizardIdentity.Modder, "Code", Wizard_Role_Modder, Wizard_Role_Modder_Description, "#047857"),
        new(WizardIdentity.Custom, "Setting", Wizard_Role_Custom, Wizard_Role_Custom_Description, "#B45309")
    ];

    public IReadOnlyList<WizardTask> Tasks { get; } =
    [
        new("MelonLoader", Wizard_Task_MelonLoader, Wizard_Task_MelonLoader_Description) { IsSelected = true },
        new("EssentialMods", Wizard_Task_EssentialMods, Wizard_Task_EssentialMods_Description) { IsSelected = true },
        new("UninstallConflicts", Wizard_Task_UninstallConflicts, Wizard_Task_UninstallConflicts_Description) { IsSelected = true },
        new("ChartingTool", Wizard_Task_ChartingTool, Wizard_Task_ChartingTool_Description),
        new("DotNetSdk", Wizard_Task_DotNetSdk, Wizard_Task_DotNetSdk_Description),
        new("ModTemplate", Wizard_Task_ModTemplate, Wizard_Task_ModTemplate_Description),
        new("EnvVariable", Wizard_Task_EnvVariable, Wizard_Task_EnvVariable_Description)
    ];

    [ObservableProperty]
    public partial bool IsProgressPage { get; set; }

    [ObservableProperty]
    public partial WizardRole? SelectedRole { get; set; }

    [ObservableProperty]
    public partial double Progress { get; set; }

    [ObservableProperty]
    public partial string ProgressDescription { get; set; } = string.Empty;

    #region Injections

    [UsedImplicitly]
    public required ILogger<WizardDialogViewModel> Logger { get; init; }

    #endregion Injections

    public WizardDialogViewModel()
    {
        SelectedRole = Roles[0];

        foreach (var task in Tasks)
        {
            task.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(WizardTask.IsSelected))
                {
                    SyncRoleFromTasks();
                }
            };
        }
    }

    public void Close() => RequestClose?.Invoke(this, EventArgs.Empty);

    public event EventHandler<object?>? RequestClose;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync().ConfigureAwait(false);
        Logger.ZLogInformation($"{nameof(WizardDialogViewModel)} Initialized");
    }

    [RelayCommand]
    private void SkipWizard() => Close();

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        var selectedTasks = Tasks.Where(t => t.IsSelected).ToList();
        var total = selectedTasks.Count;

        IsProgressPage = true;

        for (var i = 0; i < total; i++)
        {
            ProgressDescription = $"{selectedTasks[i].DisplayName} ({i + 1}/{total})";
            Progress = (double)(i + 1) / total * 100;
            await Task.Delay(Random.Shared.Next(300, 1200)).ConfigureAwait(true);
        }

        Close();
    }

    partial void OnSelectedRoleChanged(WizardRole? value)
    {
        if (_isSyncingRole || value is null)
        {
            return;
        }

        _isSyncingRole = true;
        try
        {
            if (Presets.TryGetValue(value.Identity, out var preset))
            {
                foreach (var task in Tasks)
                {
                    task.IsSelected = preset.Contains(task.Name);
                }
            }
        }
        finally
        {
            _isSyncingRole = false;
        }
    }

    private void SyncRoleFromTasks()
    {
        if (_isSyncingRole)
        {
            return;
        }

        _isSyncingRole = true;
        try
        {
            var matched = FindMatchingRole();
            SelectedRole = Roles.FirstOrDefault(o => o.Identity == matched);
        }
        finally
        {
            _isSyncingRole = false;
        }
    }

    private WizardIdentity FindMatchingRole()
    {
        if (SelectedRole is not null
            && SelectedRole.Identity is not WizardIdentity.Custom
            && Presets.TryGetValue(SelectedRole.Identity, out var currentPreset)
            && MatchesPreset(currentPreset))
        {
            return SelectedRole.Identity;
        }

        foreach (var (identity, preset) in Presets)
        {
            if (MatchesPreset(preset))
            {
                return identity;
            }
        }

        return WizardIdentity.Custom;
    }

    private bool MatchesPreset(HashSet<string> preset)
    {
        var selectedCount = 0;
        foreach (var task in Tasks)
        {
            if (!task.IsSelected)
            {
                continue;
            }

            if (!preset.Contains(task.Name))
            {
                return false;
            }

            selectedCount++;
        }

        return selectedCount == preset.Count;
    }
}