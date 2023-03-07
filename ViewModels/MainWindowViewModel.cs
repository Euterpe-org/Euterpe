using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Styling;
using DynamicData;
using MelonLoader;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Models;
using MuseDashModToolsUI.Views;
using ReactiveUI;

namespace MuseDashModToolsUI.ViewModels;

public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    public ReactiveCommand<Unit, Unit> FilterAllCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterInstalledCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterEnabledCommand { get; }
    public ReactiveCommand<Mod, Unit> SelectedItemCommand { get; }
    public ReactiveCommand<Mod, Unit> InstallModCommand { get; } 
    
    public ReactiveCommand<Mod, Unit> RemoveModCommand { get; }

    private Mod _selectedItem;

    public Mod SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    public ObservableCollection<Mod> Mods { get; } = new();
    
    private readonly IGitHubService _gitHubService;
    private readonly ILocalService _localService;

    public MainWindowViewModel()
    {
        
    }
    public MainWindowViewModel(IGitHubService gitHubService, ILocalService localService)
    {
        _gitHubService = gitHubService;
        _localService = localService;
        
        FilterAllCommand = ReactiveCommand.Create(FilterAll);
        FilterInstalledCommand = ReactiveCommand.Create(FilterInstalled);
        FilterEnabledCommand = ReactiveCommand.Create(FilterEnabled);
        SelectedItemCommand = ReactiveCommand.Create<Mod>(OnSelectedItem);
        InstallModCommand = ReactiveCommand.CreateFromTask<Mod>(OnInstallMod);
        RemoveModCommand = ReactiveCommand.CreateFromTask<Mod>(OnDeleteMod);
        
        RxApp.MainThreadScheduler.Schedule(InitializeModList);
    }

    

    private async void InitializeModList()
    {
        var webMods = await _gitHubService.GetModsAsync();
        var localPaths = _localService.GetModFiles(@"E:\SteamLibrary\steamapps\common\Muse Dash\Mods");
        var localMods = localPaths.Select(_localService.LoadMod).Where(mod => mod is not null).ToList();
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
            
            var versionDate = new Version(webMod.Version!) > new Version(localMod.Version!) ? -1 : new Version(webMod.Version!) < new Version(localMod.Version!) ? 1 : 0;
            localMod.IsShaMismatched = versionDate == 0 && webMod.SHA256 != localMod.SHA256;
            
            Mods.Add(localMod);
        }
        for (var i = 0; i < isTracked.Length; i++)
        {
            if (!isTracked[i])
            {
                if (localMods.Count(x => x!.Name == localMods[i]!.Name) == 1)
                {
                    Mods.Add(localMods[i]!);
                }
                
            }
        }
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
    private void OnSelectedItem(Mod item)
    {
        item.IsExpanded = !item.IsExpanded;
        SelectedItem = item;
    }

    private async Task OnInstallMod(Mod item)
    {
        if (item.DownloadLink is null)
        {
            await CreateMessageBox("Failure", "This mod does not have an available resource for download.", ButtonEnum.Ok, Icon.Error);
            return;
        }
        
        var errors = new StringBuilder();
        var modPaths = new List<string>();
        try
        {
            var path = Path.Join(Path.GetTempPath(), item.DownloadLink.Split("/")[1]);
            await _gitHubService.DownloadModAsync(item.DownloadLink, path);
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
                await _gitHubService.DownloadModAsync(mod, path);
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
                File.Move(path, @"E:\SteamLibrary\steamapps\common\Muse Dash\Mods\" + Path.GetFileName(path));
            }
        }
        if (errors.Length > 0)
        {
            await CreateMessageBox("Failure", errors.ToString(), ButtonEnum.Ok, Icon.Error);
            return;
        }

        await CreateMessageBox("Success", $"{item.Name} mod has been successfully installed", ButtonEnum.Ok, Icon.Info);

    }
    private void OnUpdateMod(Mod item)
    {
        
    }

    private async Task OnDeleteMod(Mod item)
    {
        var path = @"E:\SteamLibrary\steamapps\common\Muse Dash\Mods\" + item.FileName;
        if (!File.Exists(path))
        {
            await CreateMessageBox("Failure", "Cannot delete file that doesn't exist", ButtonEnum.Ok, Icon.Error);
            return;
        }

        try
        {
            File.Delete(path);
            Mods.Remove(item);

            await CreateMessageBox("Success", $"{item.Name} has been successfully deleted.", ButtonEnum.Ok, Icon.Info);
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case UnauthorizedAccessException:
                case IOException:
                    await CreateMessageBox("Failure", "Mod uninstall failed\nIs the game running?", ButtonEnum.Ok, Icon.Error);
                    break;
                default:
                    await CreateMessageBox("Failure", "Mod uninstall failed\n?", ButtonEnum.Ok, Icon.Error);
                    break;
            }
        }
        
    }

    private void OpenUrl(string path)
    {
        Process.Start(path);
    }

    private async Task<ButtonResult> CreateMessageBox(string title, string content, ButtonEnum button = ButtonEnum.Ok, Icon icon = Icon.None)
        => await MessageBoxManager
            .GetMessageBoxStandardWindow(new MessageBoxStandardParams{
                ButtonDefinitions = button,
                ContentTitle = title,
                ContentMessage = content,
                Icon = icon
            }).Show();
}