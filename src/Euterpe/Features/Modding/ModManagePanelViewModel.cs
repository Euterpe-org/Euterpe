using System.Collections.ObjectModel;
using Avalonia.Platform.Storage;
using DynamicData.Binding;

namespace Euterpe.Features.Modding;

[Route("/modding/manage", DisplayName = Panel_Modding_ModManage, Order = 0)]
public sealed partial class ModManagePanelViewModel : ViewModelBase
{
    private readonly ReadOnlyObservableCollection<ModDto> _mods;
    private readonly SourceCache<ModDto, string> _sourceCache = new(x => x.Name);

    public static IReadOnlyList<EnumOption<ModFilterType>> ModFilters { get; } =
    [
        .. ModFilterTypeExtensions.GetValues().Select(static filter =>
            new EnumOption<ModFilterType>(filter, $"{nameof(ModFilterType)}_{filter.ToStringFast()}"))
    ];

    [ObservableProperty]
    public partial ModDto SelectedMod { get; set; } = null!;

    [ObservableProperty]
    public partial bool AllModsLoaded { get; set; }

    public ModFilterViewModel Filter { get; } = new();
    public ReadOnlyObservableCollection<ModDto> Mods => _mods;

    public ModManagePanelViewModel()
    {
        var comparer = SortExpressionComparer<ModDto>
            .Descending(x => x.State is ModState.Duplicated)
            .ThenByDescending(x => x is { State: ModState.Incompatible, IsLocal: true })
            .ThenByDescending(x => x.State is ModState.Modified)
            .ThenByDescending(x => x is { IsLocal: true, IsDisabled: false })
            .ThenByDescending(x => x.IsLocal)
            .ThenByDescending(x => x is { State: ModState.Outdated, IsLocal: true })
            .ThenByDescending(x => x.IsInstallable)
            .ThenByDescending(x => x.DownloadCount)
            .ThenByAscending(x => x.Name);

        _sourceCache.Connect()
            .Filter(mod => Filter.Matches(mod))
            .SortAndBindOnUI(out _mods, comparer)
            .Subscribe();

        Filter.Changed.Subscribe(this, static (_, vm) => vm._sourceCache.Refresh());
    }

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(true);
        await ModManageService.InitializeModsAsync().ConfigureAwait(true);

        ModManageService.Connect().PopulateInto(_sourceCache);

        AllModsLoaded = true;
        Logger.LogInformation("{ViewModel} Initialized", nameof(ModManagePanelViewModel));
    }

    [RelayCommand]
    private Task OpenConfigFileAsync()
    {
        Logger.LogInformation("Opening config file for mod: {ModName}", SelectedMod.Name);
        return Launcher.OpenFileAsync(Path.Combine(GameConfig.UserDataFolder, SelectedMod.ConfigFile));
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task InstallModAsync(ModDto mod) => await ModManageService.InstallModAsync(mod).ConfigureAwait(false);

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task UpdateModAsync(ModDto mod) => await ModManageService.UpdateModAsync(mod).ConfigureAwait(false);

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task ReinstallModAsync(ModDto mod) => await ModManageService.ReinstallModAsync(mod).ConfigureAwait(false);

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task UninstallModAsync(ModDto mod) => await ModManageService.UninstallModAsync(mod).ConfigureAwait(false);

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task ToggleModAsync(ModDto mod) => await ModManageService.ToggleModAsync(mod).ConfigureAwait(false);

    [RelayCommand]
    private async Task ImportModsAsync(IReadOnlyList<IStorageItem> files)
    {
        var paths = files.GetLocalPaths().OfType<string>().ToArray();
        if (paths is [])
        {
            return;
        }

        await ModManageService.ImportModsAsync(paths).ConfigureAwait(false);
    }

    #region Injections

    public required Config Config { get; init; }
    public required GameConfig GameConfig { get; init; }
    public required ILogger<ModManagePanelViewModel> Logger { get; init; }
    public required IModManageService ModManageService { get; init; }

    #endregion Injections
}
