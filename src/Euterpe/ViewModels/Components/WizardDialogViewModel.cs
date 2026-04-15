using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.ViewModels.Components;

public sealed partial class WizardDialogViewModel : ViewModelBase, IDialogContext
{
    private bool _isSyncingIdentity;

    [ObservableProperty]
    public partial bool IsProgressPage { get; set; }

    [ObservableProperty]
    public partial WizardIdentity SelectedIdentity { get; set; }

    [ObservableProperty]
    public partial bool InstallMelonLoader { get; set; } = true;

    [ObservableProperty]
    public partial bool InstallEssentialMods { get; set; } = true;

    [ObservableProperty]
    public partial bool UninstallConflictingMods { get; set; } = true;

    [ObservableProperty]
    public partial bool DownloadChartingTool { get; set; }

    [ObservableProperty]
    public partial bool InstallDotNetSdk { get; set; }

    [ObservableProperty]
    public partial bool InstallModTemplate { get; set; }

    [ObservableProperty]
    public partial bool SetEnvironmentVariable { get; set; }

    [ObservableProperty]
    public partial bool IsCompleted { get; set; }

    [ObservableProperty]
    public partial double Progress { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<WizardTaskItem> Tasks { get; set; } = [];

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
        var tasks = new List<WizardTaskItem>();
        if (InstallMelonLoader) tasks.Add(new("Install MelonLoader"));
        if (InstallEssentialMods) tasks.Add(new("Install Essential Mods"));
        if (UninstallConflictingMods) tasks.Add(new("Uninstall Conflicting Mods"));
        if (DownloadChartingTool) tasks.Add(new("Download Charting Tool (MDBMSC)"));
        if (InstallDotNetSdk) tasks.Add(new("Install .NET SDK"));
        if (InstallModTemplate) tasks.Add(new("Install Mod Template"));
        if (SetEnvironmentVariable) tasks.Add(new("Set Environment Variable (MD_DIRECTORY)"));
        Tasks = tasks;
        IsProgressPage = true;
    }

    [RelayCommand]
    private void Complete() => Close();

    partial void OnSelectedIdentityChanged(WizardIdentity value)
    {
        if (_isSyncingIdentity) return;
        _isSyncingIdentity = true;

        (InstallMelonLoader, InstallEssentialMods, UninstallConflictingMods, DownloadChartingTool, InstallDotNetSdk, InstallModTemplate, SetEnvironmentVariable) = value switch
        {
            WizardIdentity.Player => (true, true, true, false, false, false, false),
            WizardIdentity.Charter => (true, true, true, true, false, false, false),
            WizardIdentity.Modder => (true, true, true, false, true, true, true),
            _ => (InstallMelonLoader, InstallEssentialMods, UninstallConflictingMods, DownloadChartingTool, InstallDotNetSdk, InstallModTemplate, SetEnvironmentVariable)
        };

        _isSyncingIdentity = false;
    }

    private void SyncIdentityFromComponents()
    {
        if (_isSyncingIdentity) return;
        _isSyncingIdentity = true;

        if (SelectedIdentity is not WizardIdentity.Custom && MatchesPreset(SelectedIdentity))
        {
            // Current identity still matches its preset, keep it
        }
        else
        {
            // Find the best matching preset (most specific first)
            if (MatchesPreset(WizardIdentity.Modder))
                SelectedIdentity = WizardIdentity.Modder;
            else if (MatchesPreset(WizardIdentity.Charter))
                SelectedIdentity = WizardIdentity.Charter;
            else if (MatchesPreset(WizardIdentity.Player))
                SelectedIdentity = WizardIdentity.Player;
            else
                SelectedIdentity = WizardIdentity.Custom;
        }

        _isSyncingIdentity = false;
    }

    private bool MatchesPreset(WizardIdentity identity) => identity switch
    {
        WizardIdentity.Player => InstallMelonLoader && InstallEssentialMods && UninstallConflictingMods && !DownloadChartingTool && !InstallDotNetSdk && !InstallModTemplate && !SetEnvironmentVariable,
        WizardIdentity.Charter => InstallMelonLoader && InstallEssentialMods && UninstallConflictingMods && DownloadChartingTool && !InstallDotNetSdk && !InstallModTemplate && !SetEnvironmentVariable,
        WizardIdentity.Modder => InstallMelonLoader && InstallEssentialMods && UninstallConflictingMods && !DownloadChartingTool && InstallDotNetSdk && InstallModTemplate && SetEnvironmentVariable,
        _ => false
    };

    partial void OnInstallMelonLoaderChanged(bool value) => SyncIdentityFromComponents();
    partial void OnInstallEssentialModsChanged(bool value) => SyncIdentityFromComponents();
    partial void OnUninstallConflictingModsChanged(bool value) => SyncIdentityFromComponents();
    partial void OnDownloadChartingToolChanged(bool value) => SyncIdentityFromComponents();
    partial void OnInstallDotNetSdkChanged(bool value) => SyncIdentityFromComponents();
    partial void OnInstallModTemplateChanged(bool value) => SyncIdentityFromComponents();
    partial void OnSetEnvironmentVariableChanged(bool value) => SyncIdentityFromComponents();
}
