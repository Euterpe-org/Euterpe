using System.Collections.ObjectModel;
using DynamicData;
using DynamicData.Binding;

namespace MuseDashModTools.ViewModels.Panels.Modding;

public sealed partial class ModsPanelViewModel : ViewModelBase
{
    private readonly ReadOnlyObservableCollection<ModDto> _mods;
    private readonly SourceCache<ModDto, string> _sourceCache = new(x => x.Name);
    private ModFilterType _modFilter = ModFilterType.All;

    public static string[] ModFilterTypes { get; } =
    [
        XAML_ModFilterType_All,
        XAML_ModFilterType_Installed,
        XAML_ModFilterType_Enabled,
        XAML_ModFilterType_Outdated,
        XAML_ModFilterType_Incompatible
    ];

    [ObservableProperty]
    public partial string? SearchText { get; set; }

    [ObservableProperty]
    public partial ModDto SelectedMod { get; set; } = null!;

    [ObservableProperty]
    public partial int SelectedModFilterIndex { get; set; }

    public ReadOnlyObservableCollection<ModDto> Mods => _mods;

    public ModsPanelViewModel()
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
            .Filter(x => _modFilter != ModFilterType.Installed || x.IsLocal)
            .Filter(x => _modFilter != ModFilterType.Enabled || x is { IsDisabled: false, IsLocal: true })
            .Filter(x => _modFilter != ModFilterType.Outdated || x.State == ModState.Outdated)
            .Filter(x => _modFilter != ModFilterType.Incompatible || x is { State: ModState.Incompatible, IsLocal: true })
            .SortAndBind(out _mods, comparer)
            .Subscribe();
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        Logger.ZLogInformation($"Initializing ModManagePageViewModel");
        await ModManageService.InitializeModsAsync(_sourceCache).ConfigureAwait(false);
        Logger.ZLogInformation($"ModManagePageViewModel Initialized");
    }

    [RelayCommand]
    private Task OpenConfigFileAsync()
    {
        Logger.ZLogInformation($"Opening config file for mod: {SelectedMod.Name}");
        return PlatformService.OpenFileAsync(Path.Combine(Config.UserDataFolder, SelectedMod.ConfigFile));
    }

    [RelayCommand]
    private Task UpdateModAsync()
    {
        Logger.ZLogInformation($"Updating mod: {SelectedMod.Name} from version {SelectedMod.LocalVersion} to version {SelectedMod.Version}");
        return ModManageService.InstallModAsync(SelectedMod);
    }

    [RelayCommand]
    private Task InstallModAsync()
    {
        Logger.ZLogInformation($"Installing mod: {SelectedMod.Name}");
        return ModManageService.InstallModAsync(SelectedMod);
    }

    [RelayCommand]
    private Task ReinstallModAsync()
    {
        Logger.ZLogInformation($"Reinstalling mod: {SelectedMod.Name}");
        return ModManageService.InstallModAsync(SelectedMod);
    }

    [RelayCommand]
    private Task UninstallModAsync()
    {
        Logger.ZLogInformation($"Uninstalling mod: {SelectedMod.Name}");
        return ModManageService.UninstallModAsync(SelectedMod);
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
    public Config Config { get; init; } = null!;

    [UsedImplicitly]
    public ILogger<ModsPanelViewModel> Logger { get; init; } = null!;

    [UsedImplicitly]
    public IModManageService ModManageService { get; init; } = null!;

    #endregion Injections
}