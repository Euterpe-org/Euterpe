using System.Collections.ObjectModel;
using DynamicData.Binding;

namespace Euterpe.ViewModels.Panels.Modding;

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
            .ThenByDescending(x => x.DownloadCount)
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
        await ModManageService.InitializeModsAsync().ConfigureAwait(false);

        ModManageService.Connect().PopulateInto(_sourceCache);

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
    private async Task InstallModAsync() => await ModManageService.InstallModAsync(SelectedMod).ConfigureAwait(false);

    [RelayCommand]
    private async Task UpdateModAsync() => await ModManageService.UpdateModAsync(SelectedMod).ConfigureAwait(false);

    [RelayCommand]
    private async Task ReinstallModAsync() => await ModManageService.ReinstallModAsync(SelectedMod).ConfigureAwait(false);

    [RelayCommand]
    private async Task UninstallModAsync() => await ModManageService.UninstallModAsync(SelectedMod).ConfigureAwait(false);

    [RelayCommand]
    private Task ToggleModAsync(ModDto mod)
    {
        Logger.ZLogInformation($"Toggling mod: {mod.Name}");
        return ModManageService.ToggleModAsync(mod);
    }

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

    #endregion Injections
}