using System.ComponentModel;
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
            component.PropertyChanged += OnComponentPropertyChanged;
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

    private void OnComponentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WizardComponent.IsSelected))
            SyncIdentityFromComponents();
    }

    partial void OnSelectedIdentityChanged(WizardIdentity value)
    {
        if (_isSyncingIdentity) return;
        _isSyncingIdentity = true;

        if (Presets.TryGetValue(value, out var preset))
        {
            foreach (var component in Components)
                component.IsSelected = preset.Contains(component.Name);
        }

        foreach (var option in IdentityOptions)
            option.IsSelected = option.Identity == value;

        _isSyncingIdentity = false;
    }

    private void SyncIdentityFromComponents()
    {
        if (_isSyncingIdentity) return;
        _isSyncingIdentity = true;

        var selected = GetSelectedComponentNames();

        if (SelectedIdentity is not WizardIdentity.Custom && Presets.TryGetValue(SelectedIdentity, out var current) && current.SetEquals(selected))
        {
            _isSyncingIdentity = false;
            return;
        }

        SelectedIdentity = Presets
            .Where(p => p.Value.SetEquals(selected))
            .Select(p => p.Key)
            .FirstOrDefault(WizardIdentity.Custom);

        _isSyncingIdentity = false;
    }

    private HashSet<string> GetSelectedComponentNames() =>
        Components.Where(c => c.IsSelected).Select(c => c.Name).ToHashSet();
}
