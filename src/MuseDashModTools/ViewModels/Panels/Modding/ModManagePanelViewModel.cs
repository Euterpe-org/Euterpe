using System.Collections.ObjectModel;
using DynamicData.Binding;

namespace MuseDashModTools.ViewModels.Panels.Modding;

public sealed partial class ModManagePanelViewModel : ViewModelBase
{
    private readonly ReadOnlyObservableCollection<ModDto> _mods;
    private readonly SourceCache<ModDto, string> _sourceCache = new(x => x.Name);
    private ModFilterType _modFilter = ModFilterType.All;

    public static IReadOnlyList<LocalizedString> ModFilterTypes { get; } =
    [
        ModFilterType_All,
        ModFilterType_Installed,
        ModFilterType_Enabled,
        ModFilterType_Disabled,
        ModFilterType_Outdated,
        ModFilterType_Incompatible
    ];

    [ObservableProperty]
    public partial string? SearchText { get; set; }

    [ObservableProperty]
    public partial ModDto SelectedMod { get; set; } = null!;

    [ObservableProperty]
    public partial int SelectedModFilterIndex { get; set; }

    [ObservableProperty]
    public partial bool AllModsLoaded { get; set; }

    public ReadOnlyObservableCollection<ModDto> Mods => _mods;

    public ModManagePanelViewModel()
    {
        var comparer = SortExpressionComparer<ModDto>
            .Descending(x => x.State is ModState.Duplicated)
            .ThenByDescending(x => x.State is ModState.Modified)
            .ThenByDescending(x => x is { IsLocal: true, IsDisabled: false })
            .ThenByDescending(x => x.IsLocal)
            .ThenByDescending(x => x is { State: ModState.Outdated, IsLocal: true })
            .ThenByDescending(x => x.IsInstallable)
            .ThenByAscending(x => x.Name);

        _sourceCache.Connect()
            .Filter(x => SearchText.IsNullOrEmpty()
                         || x.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                         || x.Author.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            .Filter(x => _modFilter is not ModFilterType.Installed || x.IsLocal)
            .Filter(x => _modFilter is not ModFilterType.Enabled || x is { IsDisabled: false, IsLocal: true })
            .Filter(x => _modFilter is not ModFilterType.Disabled || x is { IsDisabled: true, IsLocal: true })
            .Filter(x => _modFilter is not ModFilterType.Outdated || x.State is ModState.Outdated)
            .Filter(x => _modFilter is not ModFilterType.Incompatible || x is { State: ModState.Incompatible, IsLocal: true })
            .SortAndBind(out _mods, comparer)
            .Subscribe();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync().ConfigureAwait(false);
        await ModManageService.InitializeModsAsync(_sourceCache).ConfigureAwait(false);

        AllModsLoaded = true;
        Logger.ZLogInformation($"{nameof(ModManagePanelViewModel)} Initialized");
    }

    [RelayCommand]
    private Task OpenConfigFileAsync()
    {
        Logger.ZLogInformation($"Opening config file for mod: {SelectedMod.Name}");
        return PlatformService.OpenFileAsync(Path.Combine(Config.UserDataFolder, SelectedMod.ConfigFile));
    }

    [RelayCommand]
    private async Task InstallModAsync()
    {
        Logger.ZLogInformation($"Installing mod: {SelectedMod.Name}");
        await ModManageService.InstallModAsync(SelectedMod).ConfigureAwait(true);
        Logger.ZLogInformation($"Mod {SelectedMod.Name} successfully installed");
        NotificationService.SuccessLight(Notification_Content_Mod_Install_Success, SelectedMod.Name);
    }

    [RelayCommand]
    private async Task UpdateModAsync()
    {
        Logger.ZLogInformation($"Updating mod: {SelectedMod.Name} from version {SelectedMod.LocalVersion} to version {SelectedMod.Version}");
        File.Delete(Path.Combine(Config.ModsFolder, SelectedMod.LocalFileName));
        await ModManageService.InstallModAsync(SelectedMod).ConfigureAwait(true);
        Logger.ZLogInformation($"Mod {SelectedMod.Name} successfully updated to version {SelectedMod.Version}");
        NotificationService.SuccessLight(Notification_Content_Mod_Update_Success, SelectedMod.Name);
    }

    [RelayCommand]
    private async Task ReinstallModAsync()
    {
        Logger.ZLogInformation($"Reinstalling mod: {SelectedMod.Name}");
        File.Delete(Path.Combine(Config.ModsFolder, SelectedMod.LocalFileName));
        await ModManageService.InstallModAsync(SelectedMod).ConfigureAwait(true);
        Logger.ZLogInformation($"Mod {SelectedMod.Name} successfully reinstalled");
        NotificationService.SuccessLight(Notification_Content_Mod_Reinstall_Success, SelectedMod.Name);
    }

    [RelayCommand]
    private async Task UninstallModAsync()
    {
        Logger.ZLogInformation($"Uninstalling mod: {SelectedMod.Name}");
        await ModManageService.UninstallModAsync(SelectedMod).ConfigureAwait(true);
        Logger.ZLogInformation($"Mod {SelectedMod.Name} successfully uninstalled");
        NotificationService.SuccessLight(Notification_Content_Mod_Uninstall_Success, SelectedMod.Name);
    }

    [RelayCommand]
    private Task ToggleModAsync(ModDto mod)
    {
        Logger.ZLogInformation($"Toggling mod: {mod.Name}");
        return ModManageService.ToggleModAsync(mod);
    }

    [UsedImplicitly]
    partial void OnSelectedModFilterIndexChanged(int value)
    {
        _modFilter = (ModFilterType)value;
        _sourceCache.Refresh();
    }

    [UsedImplicitly]
    partial void OnSearchTextChanged(string? value) => _sourceCache.Refresh();

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required ILogger<ModManagePanelViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required IModManageService ModManageService { get; init; }

    [UsedImplicitly]
    public required INotificationService NotificationService { get; init; }

    #endregion Injections
}