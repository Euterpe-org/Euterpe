using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
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
    private readonly IGitHubService _gitHubService;
    private readonly ILocalService _localService;
    private readonly ILogger _logger;
    private readonly ReadOnlyObservableCollection<Mod> _mods;
    private readonly IModService _modService;
    private readonly ISettingService _settingService;

    private readonly SourceCache<Mod, string> _sourceCache = new(x => x.Name!);
    private readonly FileSystemWatcher _watcher = new();
    [ObservableProperty] private FilterType _categoryFilterType;
    [ObservableProperty] private string _filter;
    public ReadOnlyObservableCollection<Mod> Mods => _mods;

    public ModManageViewModel()
    {
    }

    public ModManageViewModel(IGitHubService gitHubService, ILocalService localService, ILogger logger, IModService modService,
        ISettingService settingService
    )
    {
        _gitHubService = gitHubService;
        _localService = localService;
        _modService = modService;
        _logger = logger;
        _settingService = settingService;


        _sourceCache.Connect()
            .Filter(x => string.IsNullOrEmpty(_filter) || x.Name!.Contains(_filter, StringComparison.OrdinalIgnoreCase) ||
                         x.XamlDescription.Contains(_filter, StringComparison.OrdinalIgnoreCase))
            .Filter(x => _categoryFilterType != FilterType.Enabled || x is { IsDisabled: false, IsLocal: true })
            .Filter(x => _categoryFilterType != FilterType.Outdated || x.State == UpdateState.Outdated)
            .Filter(x => _categoryFilterType != FilterType.Installed || x.IsLocal)
            .Filter(x => _categoryFilterType != FilterType.Incompatible || x is { IsIncompatible: true, IsLocal: true })
            .Sort(SortExpressionComparer<Mod>.Ascending(t => t.Name!))
            .Bind(out _mods)
            .Subscribe();

        Initialize();
        AppDomain.CurrentDomain.ProcessExit += OnExit!;
    }

    public async void Initialize()
    {
        await _settingService.InitializeSettings();
        await _modService.InitializeModList(_sourceCache, Mods);
        await _gitHubService.CheckUpdates();
        FileMonitorStart();
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
    private async Task OnInstallMelonLoader() => await _localService.OnInstallMelonLoader();

    [RelayCommand]
    private async Task OnUninstallMelonLoader() => await _localService.OnUninstallMelonLoader();

    [RelayCommand]
    private async Task OnChoosePath()
    {
        await _settingService.OnChoosePath();
        Initialize();
    }

    [RelayCommand]
    private void OpenUrl(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }

    [RelayCommand]
    private async Task OpenUserDataFolder() => await _localService.OpenUserDataFolder();

    [RelayCommand]
    private async Task OpenModsFolder() => await _localService.OpenModsFolder();

    [RelayCommand]
    private async Task OnCheckUpdate() => await _gitHubService.CheckUpdates(true);

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
    partial void OnCategoryFilterTypeChanged(FilterType value) => _sourceCache.Refresh();

    private void FileMonitorStart()
    {
        _watcher.Path = _settingService.Settings.ModsFolder;
        _watcher.Filters.Add("*.dll");
        _watcher.Filters.Add("*.disabled");
        _watcher.Renamed += async (_, _) => await _modService.InitializeModList(_sourceCache, Mods);
        _watcher.Changed += async (_, _) => await _modService.InitializeModList(_sourceCache, Mods);
        _watcher.Created += async (_, _) => await _modService.InitializeModList(_sourceCache, Mods);
        _watcher.Deleted += async (_, _) => await _modService.InitializeModList(_sourceCache, Mods);
        _watcher.EnableRaisingEvents = true;
    }

    private void OnExit(object sender, EventArgs e)
    {
        var json = JsonSerializer.Serialize(_settingService.Settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("Settings.json", json);
    }
}