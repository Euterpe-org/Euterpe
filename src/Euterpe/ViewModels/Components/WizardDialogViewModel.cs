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

    private bool _isSyncingIdentity;

    public static IReadOnlyList<WizardIdentityOption> IdentityOptions { get; } =
    [
        new(WizardIdentity.Player, "UserGroup", "Player", "Recommended for regular players", "#2563EB"),
        new(WizardIdentity.Charter, "Language", "Charter", "Recommended for chart creators", "#7B2CBF"),
        new(WizardIdentity.Modder, "Code", "Modder", "Recommended for mod developers", "#047857"),
        new(WizardIdentity.Custom, "Setting", "Custom", "Customize your setup", "#B45309")
    ];

    public IReadOnlyList<WizardComponent> Components { get; } =
    [
        new("MelonLoader", "Install MelonLoader") { IsSelected = true },
        new("EssentialMods", "Install Essential Mods") { IsSelected = true },
        new("UninstallConflicts", "Uninstall Conflicting Mods") { IsSelected = true },
        new("ChartingTool", "Download Charting Tool (MDBMSC)"),
        new("DotNetSdk", "Install .NET SDK"),
        new("ModTemplate", "Install Mod Template"),
        new("EnvVariable", "Set Environment Variable (MD_DIRECTORY)")
    ];

    [ObservableProperty]
    public partial bool IsProgressPage { get; set; }

    [ObservableProperty]
    public partial WizardIdentityOption? SelectedIdentityOption { get; set; }

    [ObservableProperty]
    public partial double Progress { get; set; }

    [ObservableProperty]
    public partial string ProgressDescription { get; set; } = string.Empty;

    public WizardDialogViewModel()
    {
        SelectedIdentityOption = IdentityOptions[0];

        foreach (var component in Components)
            component.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(WizardComponent.IsSelected))
                    SyncIdentityFromComponents();
            };
    }

    #region Injections

    [UsedImplicitly]
    public required ILogger<WizardDialogViewModel> Logger { get; init; }

    #endregion Injections

    public void Close() => RequestClose?.Invoke(this, null);

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
        var selectedComponents = Components.Where(c => c.IsSelected).ToList();
        var total = selectedComponents.Count;

        IsProgressPage = true;

        for (var i = 0; i < total; i++)
        {
            ProgressDescription = $"{selectedComponents[i].DisplayName} ({i + 1}/{total})";
            Progress = (double)(i + 1) / total * 100;
            await Task.Delay(Random.Shared.Next(300, 1200)).ConfigureAwait(true);
        }

        Close();
    }

    partial void OnSelectedIdentityOptionChanged(WizardIdentityOption? value)
    {
        if (_isSyncingIdentity || value is null) return;
        _isSyncingIdentity = true;
        try
        {
            if (Presets.TryGetValue(value.Identity, out var preset))
            {
                foreach (var component in Components)
                    component.IsSelected = preset.Contains(component.Name);
            }
        }
        finally
        {
            _isSyncingIdentity = false;
        }
    }

    private void SyncIdentityFromComponents()
    {
        if (_isSyncingIdentity) return;
        _isSyncingIdentity = true;
        try
        {
            var matched = FindMatchingIdentity();
            SelectedIdentityOption = IdentityOptions.FirstOrDefault(o => o.Identity == matched);
        }
        finally
        {
            _isSyncingIdentity = false;
        }
    }

    private WizardIdentity FindMatchingIdentity()
    {
        if (SelectedIdentityOption is not null
            && SelectedIdentityOption.Identity is not WizardIdentity.Custom
            && Presets.TryGetValue(SelectedIdentityOption.Identity, out var currentPreset)
            && MatchesPreset(currentPreset))
        {
            return SelectedIdentityOption.Identity;
        }

        foreach (var (identity, preset) in Presets)
        {
            if (MatchesPreset(preset))
                return identity;
        }

        return WizardIdentity.Custom;
    }

    private bool MatchesPreset(HashSet<string> preset)
    {
        var selectedCount = 0;
        foreach (var component in Components)
        {
            if (!component.IsSelected) continue;
            if (!preset.Contains(component.Name)) return false;
            selectedCount++;
        }

        return selectedCount == preset.Count;
    }
}
