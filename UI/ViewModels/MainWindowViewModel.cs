using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AssetsTools.NET.Extra;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using ICSharpCode.SharpZipLib.Zip;
using MelonLoader;
using MessageBox.Avalonia.Enums;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Models;
using ReactiveUI;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    [ObservableProperty] private string _filter;
    [ObservableProperty] private FilterType _categoryFilterType;

    private readonly SourceCache<Mod, string> _sourceCache = new(x => x.Name!);
    private readonly ReadOnlyObservableCollection<Mod> _mods;
    public ReadOnlyObservableCollection<Mod> Mods => _mods;
    private Settings _settings = new();
    private bool _isValidPath;
    private string _currentGameVersion;
    private string ModsFolder => !string.IsNullOrEmpty(_settings.MuseDashFolder) ? Path.Join(_settings.MuseDashFolder, "Mods") : string.Empty;

    private readonly IGitHubService _gitHubService;
    private readonly ILocalService _localService;
    private readonly IDialogueService _dialogueService;

    public MainWindowViewModel()
    {
    }

    public MainWindowViewModel(IGitHubService gitHubService, ILocalService localService, IDialogueService dialogueService)
    {
        _gitHubService = gitHubService;
        _localService = localService;
        _dialogueService = dialogueService;

        _sourceCache.Connect()
            .Filter(x => string.IsNullOrEmpty(_filter) || x.Name!.Contains(_filter, StringComparison.OrdinalIgnoreCase))
            .Filter(x => _categoryFilterType != FilterType.Enabled || (_categoryFilterType == FilterType.Enabled && x is { IsDisabled: false, IsLocal: true }))
            .Filter(x => _categoryFilterType != FilterType.Outdated || (_categoryFilterType == FilterType.Outdated && x.State == UpdateState.Outdated))
            .Filter(x => _categoryFilterType != FilterType.Installed || (_categoryFilterType == FilterType.Installed && x.IsLocal))
            .Filter(x => _categoryFilterType != FilterType.Incompatible || (_categoryFilterType == FilterType.Incompatible && x is { IsIncompatible: true, IsLocal: true }))
            .Sort(SortExpressionComparer<Mod>.Ascending(t => t.Name!))
            .Bind(out _mods)
            .Subscribe();

        RxApp.MainThreadScheduler.Schedule(InitializeSettings);
        //RxApp.MainThreadScheduler.Schedule(_gitHubService.CheckUpdates);
        AppDomain.CurrentDomain.ProcessExit += OnExit!;
    }

    private async void InitializeSettings()
    {
        try
        {
            if (!File.Exists("Settings.json"))
            {
                await _dialogueService.CreateErrorMessageBox("Warning", "You haven't choose Muse Dash Folder\nPlease choose the folder");
                await OnChoosePath();
                return;
            }

            var text = await File.ReadAllTextAsync("Settings.json");
            if (string.IsNullOrEmpty(text))
            {
                await _dialogueService.CreateErrorMessageBox("Warning", "Your stored Muse Dash Folder path is null\nPlease choose the correct folder");
                await OnChoosePath();
                return;
            }

            _settings = JsonSerializer.Deserialize<Settings>(text)!;
            InitializeModList();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private async Task<bool> CheckValidPath()
    {
        var exePath = Path.Join(_settings.MuseDashFolder, "MuseDash.exe");
        var gameAssemblyPath = Path.Join(_settings.MuseDashFolder, "GameAssembly.dll");
        var userDataPath = Path.Join(_settings.MuseDashFolder, "UserData");
        if (!File.Exists(exePath) || !File.Exists(gameAssemblyPath))
        {
            await _dialogueService.CreateErrorMessageBox("Couldn't find MuseDash.exe or GameAssembly.dll\nPlease choose the right folder");
            await OnChoosePath();
            return false;
        }

        try
        {
            var version = FileVersionInfo.GetVersionInfo(exePath).FileVersion;
            if (version is not "2019.4.32.16288752")
            {
                await _dialogueService.CreateErrorMessageBox("Muse Dash.exe is not correct version \nAre you using a pirated or modified version?");
                return false;
            }

            if (!Directory.Exists(ModsFolder))
                Directory.CreateDirectory(ModsFolder);

            if (!Directory.Exists(userDataPath))
                Directory.CreateDirectory(userDataPath);

            var cfgFilePath = Path.Join(_settings.MuseDashFolder, "UserData", "MuseDashModTools.cfg");
            if (!File.Exists(cfgFilePath))
                await File.WriteAllTextAsync(cfgFilePath, Process.GetCurrentProcess().MainModule!.FileName);
            else
            {
                var path = await File.ReadAllTextAsync(cfgFilePath);
                if (path != Process.GetCurrentProcess().MainModule!.FileName)
                    await File.WriteAllTextAsync(cfgFilePath, Process.GetCurrentProcess().MainModule!.FileName);
            }

            await ReadGameVersion();

            return true;
        }
        catch (Exception)
        {
            await _dialogueService.CreateErrorMessageBox("Failed to verify MuseDash.exe\nMake sure you selected the right folder");
            return false;
        }
    }

    private async Task ReadGameVersion()
    {
        var assetsManager = new AssetsManager();
        var bundlePath = Path.Join(_settings.MuseDashFolder, "MuseDash_Data", "globalgamemanagers");
        try
        {
            var instance = assetsManager.LoadAssetsFile(bundlePath, true);
            assetsManager.LoadIncludedClassPackage();
            if (!instance.file.Metadata.TypeTreeEnabled)
                assetsManager.LoadClassDatabaseFromPackage(instance.file.Metadata.UnityVersion);
            var playerSettings = instance.file.GetAssetsOfType(AssetClassID.PlayerSettings)[0];

            var bundleVersion = assetsManager.GetBaseField(instance, playerSettings)?.Get("bundleVersion");
            _currentGameVersion = bundleVersion!.AsString;
        }
        catch (Exception)
        {
            await _dialogueService.CreateErrorMessageBox($"Cannot read current game version\nDo you fully installed Muse Dash?\nPlease check your globalgamemanagers file in\n{bundlePath}");
            Environment.Exit(0);
        }
    }

    private async Task CheckMelonLoaderInstall()
    {
        var melonLoaderFolder = Path.Join(_settings.MuseDashFolder, "MelonLoader");
        var versionFile = Path.Join(_settings.MuseDashFolder, "version.dll");
        if (Directory.Exists(melonLoaderFolder) && File.Exists(versionFile)) return;
        var install = await _dialogueService.CreateConfirmMessageBox("Notice", "You did not install MelonLoader\nWhich is needed to run all the mods\nInstall Now?");
        if (install)
            await OnInstallMelonLoader();
    }

    private async void InitializeModList()
    {
        _isValidPath = await CheckValidPath();
        if (!_isValidPath) return;
        await CheckMelonLoaderInstall();

        var webMods = await _gitHubService.GetModsAsync();
        var localPaths = _localService.GetModFiles(ModsFolder);
        List<Mod>? localMods;
        try
        {
            localMods = localPaths.Select(_localService.LoadMod).Where(mod => mod is not null).ToList()!;
        }
        catch (Exception)
        {
            await _dialogueService.CreateErrorMessageBox("Your downloaded mods are broken\nPlease delete 0kb mod if it exist");
            Environment.Exit(0);
            return;
        }

        var isTracked = new bool[localMods.Count];
        foreach (var webMod in webMods)
        {
            var localMod = localMods.FirstOrDefault(x => x.Name == webMod.Name);
            var localModIdx = localMods.IndexOf(localMod!);

            if (localMod is null)
            {
                webMod.IsTracked = true;
                webMod.IsIncompatible = !CheckCompatible(webMod);
                _sourceCache.AddOrUpdate(webMod);
                await CheckModToolsInstall(webMod);
                continue;
            }

            if (localMods.Count(x => x.Name == localMod.Name) > 1)
            {
                localMod.IsDuplicated = true;
                localMod.DuplicatedModNames = string.Join("\r\n", localMods.Where(x => x.Name == localMod.Name).Select(x => x.FileNameExtended()));
            }

            isTracked[localModIdx] = true;
            localMod.IsTracked = true;
            localMod.Version = webMod.Version;
            localMod.GameVersion = webMod.GameVersion;
            localMod.DependentLibs = webMod.DependentLibs;
            localMod.DependentMods = webMod.DependentMods;
            localMod.IncompatibleMods = webMod.IncompatibleMods;
            localMod.DownloadLink = webMod.DownloadLink;
            localMod.HomePage = webMod.HomePage;

            var versionDate = new Version(webMod.Version!) > new Version(localMod.LocalVersion!) ? -1 : new Version(webMod.Version!) < new Version(localMod.LocalVersion!) ? 1 : 0;
            localMod.State = (UpdateState)versionDate;
            localMod.IsShaMismatched = versionDate == 0 && webMod.SHA256 != localMod.SHA256;
            if (localMod.IsShaMismatched)
                localMod.State = UpdateState.Modified;
            localMod.IsIncompatible = !CheckCompatible(localMod);
            _sourceCache.AddOrUpdate(localMod);
        }

        for (var i = 0; i < isTracked.Length; i++)
        {
            if (isTracked[i]) continue;
            var localMod = localMods[i];
            if (localMods.FirstOrDefault(x => x.Name == localMod.Name)!.IsTracked) continue;
            if (localMods.Count(x => x.Name == localMod.Name) > 1)
            {
                localMod.IsDuplicated = true;
                localMod.DuplicatedModNames = string.Join("\r\n", localMods.Where(x => x.Name == localMod.Name).Select(x => x.FileNameExtended()));
            }

            _sourceCache.AddOrUpdate(localMods[i]!);
        }
    }

    private bool CheckCompatible(Mod mod) => mod.CompatibleGameVersion == "All" || mod.GameVersion!.Contains(_currentGameVersion);

    private async Task CheckModToolsInstall(Mod mod)
    {
        if (_settings.AskInstallMuseDashModTools != AskType.Always) return;
        if (mod.Name != "MuseDashModTools") return;
        var result = await _dialogueService.CreateCustomConfirmMessageBox("You don't have MuseDashModTools mod installed\nWhich checks available update for all the mods when launching Muse Dash\nInstall Now?");
        switch (result)
        {
            case "Yes":
                await OnInstallMod(mod);
                break;
            case "No and Don't Ask Again":
                _settings.AskInstallMuseDashModTools = AskType.NoAndNoAsk;
                break;
        }
    }

    [RelayCommand]
    private async Task OnInstallMod(Mod item)
    {
        if (item.DownloadLink is null)
        {
            await _dialogueService.CreateErrorMessageBox("This mod does not have an available resource for download.\n");
            return;
        }

        var errors = new StringBuilder();

        try
        {
            var path = Path.Join(ModsFolder, item.IsLocal ? item.FileNameExtended() : item.DownloadLink.Split("/")[1]);
            await _gitHubService.DownloadModAsync(item.DownloadLink, path);
            var downloadedMod = _localService.LoadMod(path)!;
            var webMods = await _gitHubService.GetModsAsync();
            var mod = webMods.FirstOrDefault(x => x.Name == downloadedMod.Name)!;
            mod.FileName = downloadedMod.FileName;
            mod.LocalVersion = downloadedMod.LocalVersion;
            _sourceCache.AddOrUpdate(mod);
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case HttpRequestException:
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

        var dependencies = SearchDependencies(item.Name!).ToArray();
        foreach (var dependency in dependencies)
        {
            var installedMod = Mods.FirstOrDefault(x => x.Name == dependency.Name && x.IsLocal);
            if (installedMod is not null) continue;
            try
            {
                var path = Path.Join(ModsFolder, dependency.DownloadLink!.Split("/")[1]);
                await _gitHubService.DownloadModAsync(dependency.DownloadLink, path);
                var mod = _localService.LoadMod(path);
                _sourceCache!.AddOrUpdate(mod);
            }
            catch (Exception ex)
            {
                errors.AppendLine($"Dependency failed to install\n {ex.ToString()}");
            }
        }

        var disabledDependencies = dependencies.Where(x => x is { IsLocal: true, IsDisabled: true }).ToArray();
        if (disabledDependencies.Length > 0)
        {
            var disabledDependencyNames = string.Join(", ", disabledDependencies.Select(x => x.Name));
            _settings.AskEnableDependenciesWhenInstalling = await ChangeDependenciesState(
                $"Do you want to enable {item.Name}'s dependency {disabledDependencyNames}?",
                disabledDependencies, _settings.AskEnableDependenciesWhenInstalling, false);
        }


        if (errors.Length > 0)
        {
            await _dialogueService.CreateErrorMessageBox(errors.ToString());
            return;
        }

        await _dialogueService.CreateMessageBox("Success", $"{item.Name} has been successfully installed\n");
    }

    [RelayCommand]
    private async Task OnReinstallMod(Mod item)
    {
        if (item.State == UpdateState.Outdated)
        {
            await OnInstallMod(item);
            return;
        }

        var result = await _dialogueService.CreateConfirmMessageBox($"You are asking to reinstall {item.Name}\nPlease confirm your operation");
        if (!result) return;
        await OnInstallMod(item);
    }

    [RelayCommand]
    private async Task OnToggleMod(Mod item)
    {
        try
        {
            switch (item.IsDisabled)
            {
                case true:
                {
                    var enabledReverseDependencies = SearchReverseDependencies(item.Name!).Where(x => x is { IsLocal: true, IsDisabled: false }).ToArray();
                    if (enabledReverseDependencies.Length > 0)
                    {
                        var enabledReverseDependencyNames = string.Join(", ", enabledReverseDependencies.Select(x => x.Name));
                        var result = await _dialogueService.CreateConfirmMessageBox($"{item.Name} is used by {enabledReverseDependencyNames} as dependency\nAre you sure you want to disable this mod?");
                        if (!result)
                        {
                            item.IsDisabled = !item.IsDisabled;
                            return;
                        }

                        _settings.AskDisableDependenciesWhenDisabling = await ChangeDependenciesState(
                            $"Do you want to disable the mods depend on {item.Name} as well?",
                            enabledReverseDependencies, _settings.AskDisableDependenciesWhenDisabling, true);
                    }

                    break;
                }
                case false:
                {
                    var disabledDependencies = SearchDependencies(item.Name!).Where(x => x is { IsLocal: true, IsDisabled: true }).ToArray();
                    if (disabledDependencies.Length > 0)
                    {
                        var disabledDependencyNames = string.Join(", ", disabledDependencies.Select(x => x.Name));
                        _settings.AskEnableDependenciesWhenEnabling = await ChangeDependenciesState(
                            $"Do you want to enable {item.Name}'s dependency {disabledDependencyNames} as well?",
                            disabledDependencies, _settings.AskEnableDependenciesWhenEnabling, false);
                    }

                    break;
                }
            }

            File.Move(
                Path.Join(ModsFolder, item.FileNameExtended(true)),
                Path.Join(ModsFolder, item.FileNameExtended()));
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case UnauthorizedAccessException:
                    await _dialogueService.CreateErrorMessageBox("Mod disable/enable failed\nUnauthorized");
                    break;

                case IOException:
                    await _dialogueService.CreateErrorMessageBox("Mod disable/enable failed\nIs the game running?");
                    break;

                default:
                    await _dialogueService.CreateErrorMessageBox("Mod disable/enable failed\n");
                    break;
            }

            item.IsDisabled = !item.IsDisabled;
        }
    }

    [RelayCommand]
    private async Task OnDeleteMod(Mod item)
    {
        if (item.IsDuplicated)
        {
            await _dialogueService.CreateMessageBox("Notice", $"Please manually choose and delete the duplicated mod\n{item.DuplicatedModNames}", icon: Icon.Info);
            await OpenModsFolder();
            return;
        }

        var path = Path.Join(ModsFolder, item.FileNameExtended());
        if (!File.Exists(path))
        {
            await _dialogueService.CreateErrorMessageBox("Cannot delete file that doesn't exist");
            return;
        }

        try
        {
            var enabledReverseDependencies = SearchReverseDependencies(item.Name!).Where(x => x is { IsLocal: true, IsDisabled: false }).ToArray();
            if (enabledReverseDependencies.Length > 0)
            {
                var enabledReverseDependencyNames = string.Join(", ", enabledReverseDependencies.Select(x => x.Name));
                var result = await _dialogueService.CreateConfirmMessageBox($"{item.Name} is used by {enabledReverseDependencyNames} as dependency\nAre you sure you want to delete this mod?");
                if (!result)
                    return;
                _settings.AskDisableDependenciesWhenDeleting = await ChangeDependenciesState(
                    $"Do you want to disable the mods depend on {item.Name}?",
                    enabledReverseDependencies, _settings.AskDisableDependenciesWhenDeleting, true);
            }

            File.Delete(path);
            _sourceCache.Remove(item);
            var webMods = await _gitHubService.GetModsAsync();
            var webMod = webMods.FirstOrDefault(x => x.Name == item.Name);
            if (webMod is not null)
            {
                _sourceCache.AddOrUpdate(webMod);
            }

            await _dialogueService.CreateMessageBox("Success", $"{item.Name} has been successfully deleted.\n");
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case UnauthorizedAccessException:
                case IOException:
                    await _dialogueService.CreateErrorMessageBox("Mod uninstall failed\nIs the game running?");
                    break;

                default:
                    await _dialogueService.CreateErrorMessageBox("Mod uninstall failed");
                    break;
            }
        }
    }

    private IEnumerable<Mod> SearchDependencies(string modName)
    {
        var dependencyNames = _sourceCache.Lookup(modName).Value.DependencyNames;
        var dependencies = dependencyNames.Split("\r\n")
            .Where(x => _sourceCache.Lookup(x).HasValue)
            .Select(x => _sourceCache.Lookup(x).Value);
        return dependencies;
    }

    private IEnumerable<Mod> SearchReverseDependencies(string modName)
    {
        var reverseDependencies = _sourceCache.Items.Where(x => x.DependencyNames.Split("\r\n").Contains(modName));
        return reverseDependencies;
    }

    private async Task<AskType> ChangeDependenciesState(string content, IEnumerable<Mod> dependencies, AskType askType, bool turnOff)
    {
        switch (askType)
        {
            case AskType.Always:
                var askResult = await _dialogueService.CreateCustomConfirmMessageBox(content, 4);
                switch (askResult)
                {
                    case "Yes":
                        await ChangeState();
                        break;
                    case "Yes and Don't ask Again":
                        await ChangeState();
                        askType = AskType.YesAndNoAsk;
                        break;
                    case "No and Don't ask Again":
                        askType = AskType.NoAndNoAsk;
                        break;
                }

                break;
            case AskType.YesAndNoAsk:
                await ChangeState();
                break;
            case AskType.NoAndNoAsk:
            default: break;
        }

        async Task ChangeState()
        {
            foreach (var dependency in dependencies)
            {
                dependency.IsDisabled = turnOff;
                await OnToggleMod(dependency);
            }
        }

        return askType;
    }

    [RelayCommand]
    private async Task OnInstallMelonLoader()
    {
        if (!_isValidPath) return;
        var zipPath = Path.Join(_settings.MuseDashFolder, "MelonLoader.zip");
        if (!File.Exists(zipPath))
        {
            try
            {
                await _gitHubService.DownloadMelonLoader("MelonLoader.zip", zipPath);
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException)
                {
                    await _dialogueService.CreateErrorMessageBox("MelonLoader download failed due to internet\nAre you online?");
                    return;
                }

                await _dialogueService.CreateErrorMessageBox($"MelonLoader download failed\n{ex}");
                return;
            }
        }

        try
        {
            var fastZip = new FastZip();
            fastZip.ExtractZip(zipPath, _settings.MuseDashFolder, FastZip.Overwrite.Always, null, null, null, true);
        }
        catch (Exception)
        {
            await _dialogueService.CreateErrorMessageBox($"Cannot unzip MelonLoader.zip in\n{zipPath}\nPlease make sure your game is not running\nThen try manually unzip");
            return;
        }

        try
        {
            File.Delete(zipPath);
        }
        catch (Exception)
        {
            await _dialogueService.CreateErrorMessageBox($"Failed to delete MelonLoader.zip in\n{zipPath}\nTry manually delete");
            return;
        }

        await _dialogueService.CreateMessageBox("Success", "MelonLoader has been successfully installed\n");
    }

    [RelayCommand]
    private async Task OnUninstallMelonLoader()
    {
        if (!_isValidPath) return;
        var result = await _dialogueService.CreateConfirmMessageBox("You are asking to uninstall MelonLoader\nPlease confirm your operation");
        if (!result) return;
        var melonLoaderFolder = Path.Join(_settings.MuseDashFolder, "MelonLoader");
        var versionFile = Path.Join(_settings.MuseDashFolder, "version.dll");
        var noticeTxt = Path.Join(_settings.MuseDashFolder, "NOTICE.txt");

        if (Directory.Exists(melonLoaderFolder))
        {
            try
            {
                Directory.Delete(melonLoaderFolder, true);
                File.Delete(versionFile);
                File.Delete(noticeTxt);
                await _dialogueService.CreateMessageBox("Success", "MelonLoader has been successfully uninstalled\n");
            }
            catch (Exception)
            {
                await _dialogueService.CreateErrorMessageBox("Cannot uninstall MelonLoader\nPlease make sure your game is not running!");
            }
        }
        else
            await _dialogueService.CreateErrorMessageBox("Cannot find MelonLoader Folder\nHave you installed MelonLoader?");
    }

    [RelayCommand]
    private async Task OnChoosePath()
    {
        while (true)
        {
            var dialogue = new OpenFolderDialog { Title = "Choose Muse Dash Folder" };
            if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var path = await dialogue.ShowAsync(desktop.MainWindow);
                if (string.IsNullOrEmpty(path))
                {
                    await _dialogueService.CreateErrorMessageBox("The path you chose is invalid. Try again...");
                    continue;
                }

                _settings.MuseDashFolder = path;
                var json = JsonSerializer.Serialize(_settings);
                await File.WriteAllTextAsync("Settings.json", json);
                RxApp.MainThreadScheduler.Schedule(InitializeModList);
            }

            break;
        }
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
    private async Task OpenModsFolder()
    {
        if (!_isValidPath)
        {
            await _dialogueService.CreateErrorMessageBox("Choose correct Muse Dash folder first!");
            await OnChoosePath();
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = ModsFolder,
            UseShellExecute = true
        });
    }

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
        var json = JsonSerializer.Serialize(_settings);
        File.WriteAllText("Settings.json", json);
    }
}