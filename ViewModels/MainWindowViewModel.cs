using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using DynamicData;
using DynamicData.Binding;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Models;
using ReactiveUI;

namespace MuseDashModToolsUI.ViewModels;

public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    public ReactiveCommand<Unit, Unit> FilterAllCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterInstalledCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterEnabledCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterOutdatedCommand { get; }
    public ReactiveCommand<Mod, Unit> SelectedItemCommand { get; }
    public ReactiveCommand<Mod, Unit> InstallModCommand { get; }
    public ReactiveCommand<Mod, Unit> RemoveModCommand { get; }
    public ReactiveCommand<Mod, Unit> ToggleModCommand { get; }
    public ReactiveCommand<string, Unit> OpenUrlCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenFolderDialogueCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenModsFolderCommand { get; }
    public ReactiveCommand<Unit, Unit> InstallMelonLoaderCommand { get; }
    public ReactiveCommand<Unit, Unit> UnInstallMelonLoaderCommand { get; }

    private Mod _selectedItem;

    public Mod SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    private string _filter;

    public string Filter
    {
        get => _filter;
        set
        {
            this.RaiseAndSetIfChanged(ref _filter, value);
            _sourceCache.Refresh();
        }
    }

    private Filter _categoryFilter;

    public Filter CategoryFilter
    {
        get => _categoryFilter;
        set
        {
            this.RaiseAndSetIfChanged(ref _categoryFilter, value);
            _sourceCache.Refresh();
        }
    }

    private readonly SourceCache<Mod, string> _sourceCache = new(x => x.Name!);
    private readonly ReadOnlyObservableCollection<Mod> _mods;
    public ReadOnlyObservableCollection<Mod> Mods => _mods;
    private Settings _settings = new();

    private readonly IGitHubService _gitHubService;
    private readonly ILocalService _localService;

    public MainWindowViewModel()
    {
    }

    public MainWindowViewModel(IGitHubService gitHubService, ILocalService localService)
    {
        _gitHubService = gitHubService;
        _localService = localService;

        FilterAllCommand = ReactiveCommand.Create(OnFilterAll);
        FilterInstalledCommand = ReactiveCommand.Create(OnFilterInstalled);
        FilterEnabledCommand = ReactiveCommand.Create(OnFilterEnabled);
        FilterOutdatedCommand = ReactiveCommand.Create(OnFilterOutdated);

        OpenUrlCommand = ReactiveCommand.Create<string>(OpenUrl);
        OpenFolderDialogueCommand = ReactiveCommand.CreateFromTask(OnChoosePath);
        OpenModsFolderCommand = ReactiveCommand.CreateFromTask(OpenModsFolder);
        InstallMelonLoaderCommand = ReactiveCommand.CreateFromTask(OnInstallMelonLoader);
        UnInstallMelonLoaderCommand = ReactiveCommand.CreateFromTask(OnUnInstallMelonLoader);


        SelectedItemCommand = ReactiveCommand.Create<Mod>(OnSelectedItem);
        InstallModCommand = ReactiveCommand.CreateFromTask<Mod>(OnInstallMod);
        RemoveModCommand = ReactiveCommand.CreateFromTask<Mod>(OnDeleteMod);
        ToggleModCommand = ReactiveCommand.CreateFromTask<Mod>(OnToggleMod);

        _sourceCache.Connect()
            .Filter(x => string.IsNullOrEmpty(Filter) || x.Name!.Contains(Filter, StringComparison.OrdinalIgnoreCase))
            .Filter(x => CategoryFilter != Models.Filter.Enabled || (CategoryFilter == Models.Filter.Enabled && x is { IsDisabled: false, IsLocal: true }))
            .Filter(x => CategoryFilter != Models.Filter.Outdated || (CategoryFilter == Models.Filter.Outdated && x.State == UpdateState.Outdated))
            .Filter(x => CategoryFilter != Models.Filter.Installed || (CategoryFilter == Models.Filter.Installed && x.IsLocal))
            .Sort(SortExpressionComparer<Mod>.Ascending(t => t.Name!))
            .Bind(out _mods)
            .Subscribe();

        InitializeSettings();
        if (!string.IsNullOrEmpty(_settings.ModsFolder))
        {
            RxApp.MainThreadScheduler.Schedule(InitializeModList);
        }
    }


    private async void InitializeModList()
    {
        var webMods = await _gitHubService.GetModsAsync();
        var localPaths = _localService.GetModFiles(_settings.ModsFolder);
        var localMods = localPaths.Select(_localService.LoadMod).Where(mod => mod is not null).ToList();
        var isTracked = new bool[localMods.Count];
        foreach (var webMod in webMods)
        {
            var localMod = localMods.FirstOrDefault(x => x!.Name == webMod.Name);
            var localModIdx = localMods.IndexOf(localMod);

            if (localMod is null)
            {
                webMod.IsTracked = true;
                _sourceCache.AddOrUpdate(webMod);
                continue;
            }

            if (localMods.Count(x => x!.Name == localMod.Name) > 1)
            {
                localMod.IsDuplicated = true;
            }

            isTracked[localModIdx] = true;
            localMod.IsTracked = true;
            localMod.Version = webMod.Version;

            localMod.DependentLibs = webMod.DependentLibs;
            localMod.DependentMods = webMod.DependentMods;
            localMod.IncompatibleMods = webMod.IncompatibleMods;
            localMod.DownloadLink = webMod.DownloadLink;
            localMod.HomePage = webMod.HomePage;

            var versionDate = new Version(webMod.Version!) > new Version(localMod.LocalVersion!) ? -1 : new Version(webMod.Version!) < new Version(localMod.LocalVersion!) ? 1 : 0;
            localMod.State = (UpdateState)versionDate;
            localMod.IsShaMismatched = versionDate == 0 && webMod.SHA256 != localMod.SHA256;

            _sourceCache.AddOrUpdate(localMod);
        }

        for (var i = 0; i < isTracked.Length; i++)
        {
            if (!isTracked[i])
            {
                if (localMods.Count(x => x!.Name == localMods[i]!.Name) == 1)
                {
                    _sourceCache.AddOrUpdate(localMods[i]!);
                }
            }
        }
    }

    private void InitializeSettings()
    {
        try
        {
            if (!File.Exists("appsettings.json"))
            {
                File.Create("appsettings.json");
                var json = JsonSerializer.Serialize(_settings);
                File.WriteAllText("appsettings.json", json);
            }

            var options = JsonSerializer.Deserialize<Settings>(File.ReadAllText("appsettings.json"));
            _settings = options;
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private async Task OnInstallMod(Mod item)
    {
        if (item.DownloadLink is null)
        {
            await CreateErrorMessageBox("Failure", "This mod does not have an available resource for download.");
            return;
        }

        var errors = new StringBuilder();
        var modPaths = new List<string>();
        var mods = new List<Mod>();
        try
        {
            var path = Path.Join(Path.GetTempPath(), item.IsLocal ? item.FileNameExtended() : item.DownloadLink.Split("/")[1]);
            if (!File.Exists(path))
            {
                await _gitHubService.DownloadModAsync(item.DownloadLink, path);
            }

            modPaths.Add(path);
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case WebException:
                    errors.AppendLine($"Mod installation failed\nAre you online? {ex.ToString()}");
                    break;
                case SecurityException:
                case UnauthorizedAccessException:
                case IOException:
                    errors.AppendLine($"Mod installation failed\nIs the game running? {ex.ToString()}");
                    break;
                default:
                    errors.AppendLine($"Mod installation failed\n{ex.ToString()}");
                    break;
            }
        }

        foreach (var mod in item.DependentMods)
        {
            var dependedMod = Mods.FirstOrDefault(x => x.DownloadLink == mod && x.IsLocal);
            if (dependedMod is not null) continue;
            try
            {
                var path = Path.Join(Path.GetTempPath(), mod.Split("/")[1]);
                if (!File.Exists(path))
                {
                    await _gitHubService.DownloadModAsync(mod, path);
                }

                modPaths.Add(path);
            }
            catch (Exception ex)
            {
                errors.AppendLine($"Dependency failed to install\n {ex.ToString()}");
            }
        }


        if (modPaths.Count > 0)
        {
            foreach (var path in modPaths)
            {
                var fullPath = Path.Join(_settings.ModsFolder, Path.GetFileName(path));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                File.Move(path, fullPath);
                var mod = _localService.LoadMod(fullPath);
                if (mod is not null)
                {
                    mods.Add(mod);
                }
            }
        }

        if (errors.Length > 0)
        {
            await CreateErrorMessageBox("Failure", errors.ToString());
            return;
        }

        if (mods.Count > 0)
        {
            foreach (var mod in mods)
            {
                var existedMod = _mods.FirstOrDefault(x => x.Name == mod.Name);
                if (existedMod is not null)
                {
                    _sourceCache.Remove(existedMod);
                }

                _sourceCache.AddOrUpdate(mod);
            }
        }

        await CreateMessageBox("Success", $"{item.Name} has been successfully installed", ButtonEnum.Ok, Icon.Info);
    }

    private async Task OnToggleMod(Mod item)
    {
        //Kind of a bummer that I have to reverse the boolean here, due to binding triggering before the command executes. If you find a better way for this, hit me with a big fat PR
        item.IsDisabled = !item.IsDisabled;
        try
        {
            File.Move(
                Path.Join(_settings.ModsFolder, item.FileNameExtended()),
                Path.Join(_settings.ModsFolder, item.FileNameExtended(true)));
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case UnauthorizedAccessException:
                    await CreateErrorMessageBox("Failure", "Mod disable/enable failed\nUnauthorized");
                    break;
                case IOException:
                    await CreateErrorMessageBox("Failure", "Mod disable/enable failed\nIs the game running?");
                    break;

                default:
                    await CreateErrorMessageBox("Failure", "Mod disable/enable failed\n");
                    break;
            }

            item.IsDisabled = !item.IsDisabled;
        }

        item.IsDisabled = !item.IsDisabled;
    }

    private async Task OnDeleteMod(Mod item)
    {
        var path = Path.Join(_settings.ModsFolder, item.FileNameExtended());
        if (!File.Exists(path))
        {
            await CreateErrorMessageBox("Failure", "Cannot delete file that doesn't exist");
            return;
        }

        try
        {
            File.Delete(path);
            _sourceCache.Remove(item);

            var mods = await _gitHubService.GetModsAsync();
            var webMod = mods.FirstOrDefault(x => x.Name == item.Name);
            if (webMod is not null)
            {
                _sourceCache.AddOrUpdate(webMod);
            }

            await CreateMessageBox("Success", $"{item.Name} has been successfully deleted.", ButtonEnum.Ok, Icon.Info);
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case UnauthorizedAccessException:
                case IOException:
                    await CreateErrorMessageBox("Failure", "Mod uninstall failed\nIs the game running?");
                    break;
                default:
                    await CreateErrorMessageBox("Failure", "Mod uninstall failed\n?");
                    break;
            }
        }
    }

    private async Task OnInstallMelonLoader()
    {
        /* we need to upload a zip called "MelonLoader.zip" into github modlinks repository
        and download from github (most likely about 50mb)
        then unzip it to muse dash folder*/
        var MuseDashFolder = _settings.ModsFolder[..^5];
    }

    private async Task OnUnInstallMelonLoader()
    {
        var MuseDashFolder = _settings.ModsFolder[..^5];
        var MelonLoaderFolder = Path.Join(MuseDashFolder, "MelonLoader");
        var versionFile = Path.Join(MuseDashFolder, "version.dll");
        var noticeTxt = Path.Join(MuseDashFolder, "NOTICE.txt");

        if (Directory.Exists(MelonLoaderFolder))
        {
            try
            {
                Directory.Delete(MelonLoaderFolder, true);
                File.Delete(versionFile);
                File.Delete(noticeTxt);
                await CreateMessageBox("Success", "MelonLoader has been successfully uninstalled", ButtonEnum.Ok, Icon.Success);
            }
            catch (Exception)
            {
                await CreateErrorMessageBox("Failure", "Cannot uninstall MelonLoader, please make sure your game is not running!");
            }
        }
        else
            await CreateErrorMessageBox("Failure", "Cannot find MelonLoader Folder, have you installed MelonLoader?");
    }

    private async Task OnChoosePath()
    {
        while (true)
        {
            var dialogue = new OpenFolderDialog();
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var path = await dialogue.ShowAsync(desktop.MainWindow);
                if (string.IsNullOrEmpty(path))
                {
                    await CreateErrorMessageBox("Failure", "The path you chose is invalid. Try again...");
                    continue;
                }

                _settings.ModsFolder = path;
                var json = JsonSerializer.Serialize(_settings);
                await File.WriteAllTextAsync("appsettings.json", json);
                RxApp.MainThreadScheduler.Schedule(InitializeModList);
            }

            break;
        }
    }

    private void OpenUrl(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }

    private async Task OpenModsFolder()
    {
        if (string.IsNullOrEmpty(_settings.ModsFolder))
        {
            await CreateErrorMessageBox("Failure", "Choose the mods folder first!");
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = _settings.ModsFolder,
            UseShellExecute = true
        });
    }


    private void OnFilterAll() => CategoryFilter = Models.Filter.All;

    private void OnFilterInstalled() => CategoryFilter = Models.Filter.Installed;

    private void OnFilterEnabled() => CategoryFilter = Models.Filter.Enabled;

    private void OnFilterOutdated() => CategoryFilter = Models.Filter.Outdated;

    private void OnSelectedItem(Mod item)
    {
        item.IsExpanded = !item.IsExpanded;
        SelectedItem = item;
    }

    private async Task<ButtonResult> CreateMessageBox(string title, string content, ButtonEnum button = ButtonEnum.Ok, Icon icon = Icon.None)
        => await MessageBoxManager
            .GetMessageBoxStandardWindow(new MessageBoxStandardParams
            {
                ButtonDefinitions = button,
                ContentTitle = title,
                ContentMessage = content,
                Icon = icon
            }).Show();

    private async Task<ButtonResult> CreateErrorMessageBox(string title, string content) => await CreateMessageBox(title, content, icon: Icon.Error);
}