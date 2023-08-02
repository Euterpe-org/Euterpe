using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Models;
using Serilog;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.ViewModels.Tabs;

public partial class ModManageViewModel : ViewModelBase, IModManageViewModel
{
    private readonly ILogger _logger;
    private readonly ReadOnlyObservableCollection<Mod> _mods;
    private readonly IModService _modService;
    private readonly ISavingService _savingService;

    private readonly SourceCache<Mod, string> _sourceCache = new(x => x.Name!);
    private readonly FileSystemWatcher _watcher = new();
    [ObservableProperty] private FilterType _categoryFilterType;
    [ObservableProperty] private string _filter;
    public IGitHubService GitHubService { get; init; }
    public ILocalService LocalService { get; init; }
    public ReadOnlyObservableCollection<Mod> Mods => _mods;

    public ModManageViewModel(ILogger logger, IModService modService, ISavingService savingService)
    {
        _logger = logger;
        _modService = modService;
        _savingService = savingService;

        _sourceCache.Connect()
            .Filter(x => string.IsNullOrEmpty(_filter) ||
                         x.Name!.Contains(_filter, StringComparison.OrdinalIgnoreCase) ||
                         x.XamlDescription.Contains(_filter, StringComparison.OrdinalIgnoreCase))
            .Filter(x => _categoryFilterType != FilterType.Enabled || x is { IsDisabled: false, IsLocal: true })
            .Filter(x => _categoryFilterType != FilterType.Outdated || x.State == UpdateState.Outdated)
            .Filter(x => _categoryFilterType != FilterType.Installed || x.IsLocal)
            .Filter(x => _categoryFilterType != FilterType.Incompatible || x is { IsIncompatible: true, IsLocal: true })
            .Sort(SortExpressionComparer<Mod>.Ascending(t => t.Name!))
            .Bind(out _mods)
            .Subscribe();

        Initialize();
    }

    public async void Initialize()
    {
        await _savingService.InitializeSettings();
        await _modService.InitializeModList(_sourceCache, Mods);
        StartModsDllMonitor();
        _logger.Information("Mod Manage Window Initialized");
    }

    [RelayCommand]
    private async Task OnInstallMod(Mod item)
    {
        _watcher.EnableRaisingEvents = false;
        await _modService.OnInstallMod(item);
        _watcher.EnableRaisingEvents = true;
    }

    [RelayCommand]
    private async Task OnReinstallMod(Mod item)
    {
        _watcher.EnableRaisingEvents = false;
        await _modService.OnReinstallMod(item);
        _watcher.EnableRaisingEvents = true;
    }

    [RelayCommand]
    private async Task OnToggleMod(Mod item)
    {
        _watcher.EnableRaisingEvents = false;
        await _modService.OnToggleMod(item);
        _watcher.EnableRaisingEvents = true;
    }

    [RelayCommand]
    private async Task OnDeleteMod(Mod item)
    {
        _watcher.EnableRaisingEvents = false;
        await _modService.OnDeleteMod(item);
        _watcher.EnableRaisingEvents = true;
    }

    [RelayCommand]
    private async Task OnInstallMelonLoader() => await LocalService.OnInstallMelonLoader();

    [RelayCommand]
    private async Task OnUninstallMelonLoader() => await LocalService.OnUninstallMelonLoader();

    [RelayCommand]
    private void OpenUrl(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
        _logger.Information("Open Url: {Url}", path);
    }

    [RelayCommand]
    private async Task OpenUserDataFolder() => await LocalService.OpenUserDataFolder();

    [RelayCommand]
    private async Task OpenModsFolder() => await LocalService.OpenModsFolder();

    [RelayCommand]
    private async Task OnCheckUpdate() => await GitHubService.CheckUpdates(true);

    [RelayCommand]
    private void OnFilterAll() => CategoryFilterType = FilterType.All;

    [RelayCommand]
    private void OnFilterInstalled() => CategoryFilterType = FilterType.Installed;

    [RelayCommand]
    private void OnFilterEnabled() => CategoryFilterType = FilterType.Enabled;

    [RelayCommand]
    private void OnFilterOutdated() => CategoryFilterType = FilterType.Outdated;

    [RelayCommand]
    private void OnFilterIncompatible() => CategoryFilterType = FilterType.Incompatible;

    partial void OnFilterChanged(string value) => _sourceCache.Refresh();

    partial void OnCategoryFilterTypeChanged(FilterType value)
    {
        _logger.Information("Category Filter Changed: {Filter}", value);
        _sourceCache.Refresh();
    }

    private void StartModsDllMonitor()
    {
        _watcher.Path = _savingService.Settings.ModsFolder;
        _watcher.Filters.Add("*.dll");
        _watcher.Filters.Add("*.disabled");
        _watcher.Renamed += async (_, _) => await _modService.InitializeModList(_sourceCache, Mods);
        _watcher.Changed += async (_, _) => await _modService.InitializeModList(_sourceCache, Mods);
        _watcher.Created += async (_, _) => await _modService.InitializeModList(_sourceCache, Mods);
        _watcher.Deleted += async (_, _) => await _modService.InitializeModList(_sourceCache, Mods);
        _watcher.EnableRaisingEvents = true;
        _logger.Information("Mods Dll File Monitor Started");
    }
}