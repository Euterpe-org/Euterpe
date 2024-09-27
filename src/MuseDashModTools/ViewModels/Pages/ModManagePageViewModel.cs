using System.Collections.ObjectModel;
using DynamicData;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class ModManagePageViewModel : ViewModelBase
{
    private readonly ReadOnlyObservableCollection<ModDto> _mods;
    private readonly SourceCache<ModDto, string> _sourceCache = new(x => x.Name);
    private ModFilterType _modFilterType;
    [ObservableProperty] private string? _searchText;
    public ReadOnlyObservableCollection<ModDto> Mods => _mods;

    public ModManagePageViewModel()
    {
        _sourceCache.Connect()
            .Filter(x => SearchText.IsNullOrEmpty() ||
                         x.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                         x.XamlDescription.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            .Filter(x => _modFilterType != ModFilterType.Installed || x.IsLocal)
            .Filter(x => _modFilterType != ModFilterType.Enabled || x is { IsDisabled: false, IsLocal: true })
            .Filter(x => _modFilterType != ModFilterType.Outdated || x.State == ModState.Outdated)
            .Filter(x => _modFilterType != ModFilterType.Incompatible || x is { State: ModState.Incompatible, IsLocal: true })
            .SortBy(x => x.Name)
            .Bind(out _mods)
            .Subscribe();
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        Logger.Information("Initializing ModManagePageViewModel");
        await ModManageService.InitializeModsAsync(_sourceCache).ConfigureAwait(false);
        Logger.Information("ModManagePageViewModel Initialized");
    }

    [RelayCommand]
    private void FilterMods(ModFilterType filterType) => _modFilterType = filterType;

    #region Injections

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    [UsedImplicitly]
    public IModManageService ModManageService { get; init; } = null!;

    #endregion Injections
}