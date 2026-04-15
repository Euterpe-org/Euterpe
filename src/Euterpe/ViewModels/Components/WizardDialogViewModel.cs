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

    public IReadOnlyList<WizardIdentityOption> IdentityOptions { get; } =
    [
        new(WizardIdentity.Player, "UserGroup", "Player", "Recommended for regular players") { IsSelected = true },
        new(WizardIdentity.Charter, "Language", "Charter", "Recommended for chart creators"),
        new(WizardIdentity.Modder, "Code", "Modder", "Recommended for mod developers"),
        new(WizardIdentity.Custom, "Setting", "Custom", "Customize your setup")
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
    public partial WizardIdentity SelectedIdentity { get; set; }

    [ObservableProperty]
    public partial bool IsCompleted { get; set; }

    [ObservableProperty]
    public partial double Progress { get; set; }

    [ObservableProperty]
    public partial string ProgressDescription { get; set; } = string.Empty;

    public WizardDialogViewModel()
    {
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
    private void SelectIdentity(WizardIdentity identity) => SelectedIdentity = identity;

    [RelayCommand]
    private void SkipWizard() => Close();

    [RelayCommand]
    private void Confirm()
    {
        IsProgressPage = true;
        ProgressDescription = "Preparing... (0/0)";
    }

    [RelayCommand]
    private void Complete() => Close();

    partial void OnSelectedIdentityChanged(WizardIdentity value)
    {
        if (_isSyncingIdentity) return;
        _isSyncingIdentity = true;
        try
        {
            if (Presets.TryGetValue(value, out var preset))
            {
                foreach (var component in Components)
                    component.IsSelected = preset.Contains(component.Name);
            }

            UpdateIdentitySelections(value);
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
            if (matched != SelectedIdentity)
                SelectedIdentity = matched;

            UpdateIdentitySelections(SelectedIdentity);
        }
        finally
        {
            _isSyncingIdentity = false;
        }
    }

    private WizardIdentity FindMatchingIdentity()
    {
        // Preserve current identity if its preset still matches
        if (SelectedIdentity is not WizardIdentity.Custom
            && Presets.TryGetValue(SelectedIdentity, out var currentPreset)
            && MatchesPreset(currentPreset))
        {
            return SelectedIdentity;
        }

        foreach (var (identity, preset) in Presets)
        {
            if (MatchesPreset(preset))
                return identity;
        }

        return WizardIdentity.Custom;
    }

    /// <summary>
    ///     Zero-allocation single-pass comparison against a preset.
    /// </summary>
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

    private void UpdateIdentitySelections(WizardIdentity active)
    {
        foreach (var option in IdentityOptions)
            option.IsSelected = option.Identity == active;
    }
}
