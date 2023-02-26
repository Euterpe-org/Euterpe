using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Threading.Tasks;
using MelonLoader;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Models;
using MuseDashModToolsUI.Views;
using ReactiveUI;
using System.Reactive.Concurrency;

namespace MuseDashModToolsUI.ViewModels;

internal class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    public ReactiveCommand<Unit, Unit> FilterAllCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterInstalledCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterEnabledCommand { get; }

    public ObservableCollection<Mod> Mods { get; } = new();
    private readonly IGitHubService _gitHubService;
    private readonly ILocalService _localService;

    public MainWindowViewModel(IGitHubService gitHubService, ILocalService localService)
    {
        _gitHubService = gitHubService;
        _localService = localService;
        
        FilterAllCommand = ReactiveCommand.Create(FilterAll);
        FilterInstalledCommand = ReactiveCommand.Create(FilterInstalled);
        FilterEnabledCommand = ReactiveCommand.Create(FilterEnabled);

        RxApp.MainThreadScheduler.Schedule(InitializeModList);
    }

    private async void InitializeModList()
    {
        var webMods = await _gitHubService.GetModsAsync();
        var localPaths = _localService.GetModFiles(@"E:\SteamLibrary\steamapps\common\Muse Dash\Mods");
        var localMods = localPaths.Select(LoadLocalMod).Where(mod => mod is not null).ToList();
        var isTracked = new bool[localMods.Count];
        foreach (var webMod in webMods)
        {
            var localMod = localMods.FirstOrDefault(x => x!.Name == webMod.Name);
            var localModIdx = localMods.IndexOf(localMod);
            
            if (localMod is null)
            {
                webMod.IsTracked = true;
                Mods.Add(webMod);
                continue;
            }
            
            
            isTracked[localModIdx] = true;
            localMod.IsTracked = true;
            var versionDate = new Version(webMod.Version!) > new Version(localMod.Version!) ? -1 : new Version(webMod.Version!) < new Version(localMod.Version!) ? 1 : 0;
            localMod.IsShaMismatched = versionDate == 0 && webMod.SHA256 != localMod.SHA256;
            
            Mods.Add(localMod);
        }
        for (var i = 0; i < isTracked.Length; i++)
        {
            if (!isTracked[i])
            {
                Mods.Add(localMods[i]!);
            }
        }
        
    }
    
    private Mod? LoadLocalMod(string file)
    {
        var mod = new Mod
        {
            IsDisabled = file.EndsWith(".disabled"),
        };

        mod.FileName = mod.IsDisabled ? Path.GetFileName(file)[..^9] : Path.GetFileName(file);
        var assembly = Assembly.Load(File.ReadAllBytes(file));
        var attribute = MelonUtils.PullAttributeFromAssembly<MelonInfoAttribute>(assembly);

        mod.Name = attribute.Name;
        mod.Version = attribute.Version;
        if (mod.Name == null || mod.Version == null)
        {
            return null;
        }
        mod.Author = attribute.Author;
        mod.HomePage = attribute.DownloadLink;
        mod.SHA256 = MelonUtils.ComputeSimpleSHA256Hash(file);

        return mod;
    }
    
    

    public void FilterAll()
    {
        MainWindow.Instance!.Selected_ModFilter = 0;
        MainWindow.Instance.UpdateFilters();
    }

    public void FilterInstalled()
    {
        MainWindow.Instance!.Selected_ModFilter = 1;
        MainWindow.Instance.UpdateFilters();
    }

    public void FilterEnabled()
    {
        MainWindow.Instance!.Selected_ModFilter = 2;
        MainWindow.Instance.UpdateFilters();
    }

    public void FilterOutdated()
    {
        MainWindow.Instance!.Selected_ModFilter = 3;
        MainWindow.Instance.UpdateFilters();
    }
}