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
        _sourceCache.Connect()
            // TODO Try Search Values after .net9 (lxy, 2024/10/2)
            .Filter(x => SearchText.IsNullOrEmpty() ||
                         x.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
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

    partial void OnModFilterChanged(ModFilterType value) => _sourceCache.Refresh();

    partial void OnSearchTextChanged(string value) => _sourceCache.Refresh();

    #region Injections

    [UsedImplicitly]
    public ILogger<ModsPanelViewModel> Logger { get; init; } = null!;

    [UsedImplicitly]
    public IModManageService ModManageService { get; init; } = null!;

    #endregion Injections
}