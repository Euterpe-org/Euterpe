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

#pragma warning disable CS8618

namespace MuseDashModToolsUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    [ObservableProperty] private string _filter;
    [ObservableProperty] private FilterType _categoryFilterType;

    private readonly SourceCache<Mod, string> _sourceCache = new(x => x.Name!);
    private readonly ReadOnlyObservableCollection<Mod> _mods;
    public ReadOnlyObservableCollection<Mod> Mods => _mods;

    private readonly IGitHubService _gitHubService;
    private readonly ISettingService _settings;
    private readonly ILocalService _localService;
    private readonly IModService _modService;

    public MainWindowViewModel(IGitHubService gitHubService, ISettingService settings, ILocalService localService, IModService modService)
    {
        _gitHubService = gitHubService;
        _settings = settings;
        _localService = localService;
        _modService = modService;

        _sourceCache.Connect()
            .Filter(x => string.IsNullOrEmpty(_filter) || x.Name!.Contains(_filter, StringComparison.OrdinalIgnoreCase))
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

    private async void Initialize()
    {
        await _settings.InitializeSettings();
        await _modService.InitializeModList(_sourceCache, Mods);
        await _gitHubService.CheckUpdates();
    }

    [RelayCommand]
    private async Task OnInstallMod(Mod item) => await _modService.OnInstallMod(item);

    [RelayCommand]
    private async Task OnReinstallMod(Mod item) => await _modService.OnReinstallMod(item);

    [RelayCommand]
    private async Task OnToggleMod(Mod item) => await _modService.OnToggleMod(item);

    [RelayCommand]
    private async Task OnDeleteMod(Mod item) => await _modService.OnDeleteMod(item);

    [RelayCommand]
    private async Task OnInstallMelonLoader() => await _localService.OnInstallMelonLoader();

    [RelayCommand]
    private async Task OnUninstallMelonLoader() => await _localService.OnUninstallMelonLoader();

    [RelayCommand]
    private async Task OnChoosePath() => await _settings.OnChoosePath();

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

    private void OnExit(object sender, EventArgs e)
    {
        var json = JsonSerializer.Serialize(_settings.Settings);
        File.WriteAllText("Settings.json", json);
    }
}