using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;

namespace MuseDashModTools.ViewModels.Panels.Modding;

public sealed partial class ModsPanelViewModel : ViewModelBase
{
    private readonly ReadOnlyObservableCollection<ModDto> _mods;
    private readonly SourceCache<ModDto, string> _sourceCache = new(x => x.Name);

    [ObservableProperty]
    public partial string? SearchText { get; set; }

    [ObservableProperty]
    public partial ModDto? SelectedMod { get; set; }

    [ObservableProperty]
    public partial ModFilterType ModFilter { get; set; } =  ModFilterType.All;

    public static Array ModFilterTypes => Enum.GetValues<ModFilterType>();
    public ReadOnlyObservableCollection<ModDto> Mods => _mods;

    public ModsPanelViewModel()
    {
        var comparer = SortExpressionComparer<ModDto>
            .Descending(x => x.IsDuplicated)
            .ThenByDescending(x => x.State is ModState.Modified)
            .ThenByDescending(x => x.State is ModState.Outdated && !x.IsDisabled)
            .ThenByDescending(x => !x.IsDisabled)
            .ThenByDescending(x => x.State is ModState.Outdated)
            .ThenByDescending(x => x.IsLocal)
            .ThenByDescending(x => x.IsInstallable)
            .ThenByAscending(x => x.Name);

        _sourceCache.Connect()
            .Filter(x => SearchText.IsNullOrEmpty()
                         || x.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                         || x.Author.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            .Filter(x => ModFilter != ModFilterType.Installed || x.IsLocal)
            .Filter(x => ModFilter != ModFilterType.Enabled || x is { IsDisabled: false, IsLocal: true })
            .Filter(x => ModFilter != ModFilterType.Outdated || x.State == ModState.Outdated)
            .Filter(x => ModFilter != ModFilterType.Incompatible || x is { State: ModState.Incompatible, IsLocal: true })
            .Sort(comparer)
            .Bind(out _mods)
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
    private void OpenConfigFile()
    {
    }

    [RelayCommand]
    private void UpdateMod()
    {
    }

    [RelayCommand]
    private void InstallMod()
    {
    }

    [RelayCommand]
    private void ReinstallMod()
    {
    }

    [RelayCommand]
    private void UninstallMod()
    {
    }

    [RelayCommand]
    private void OpenDownloadLink()
    {
    }

    [RelayCommand]
    private void OpenGitHubRepo()
    {
    }

    [RelayCommand]
    private void ToggleModState(ModDto mod)
    {
    }

    partial void OnModFilterChanged(ModFilterType value) => _sourceCache.Refresh();

    partial void OnSearchTextChanged(string value) => _sourceCache.Refresh();

    #region Injections

    [UsedImplicitly]
    public ILogger<ModsPanelViewModel> Logger { get; init; } = null!;

    [UsedImplicitly]
    public IModManageService ModManageService { get; init; } = null!;

    #endregion Injections
}