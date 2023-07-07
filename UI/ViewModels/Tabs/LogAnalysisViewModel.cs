using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Extensions;
using Serilog;
using static MuseDashModToolsUI.Localization.Resources;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.ViewModels.Tabs;

public partial class LogAnalysisViewModel : ViewModelBase, ILogAnalysisViewModel
{
    private readonly ILocalService _localService;
    private readonly ILogger _logger;
    private readonly ISettingService _settingService;
    private readonly FileSystemWatcher _watcher = new();
    [ObservableProperty] private string _logContent;

    public IMessageBoxService MessageBoxService { get; init; }

    public LogAnalysisViewModel(ILocalService localService, ILogger logger, ISettingService settingService)
    {
        _localService = localService;
        _logger = logger;
        _settingService = settingService;
        Initialize();
    }

    public async void Initialize()
    {
        LogContent = await _localService.LoadLog();
        StartLogFileMonitor();
        _logger.Information("Log Analysis Window Initialized");
    }

    [RelayCommand]
    private async Task AnalyzeLog()
    {
        var pirate = await _localService.CheckPirate(LogContent);
        if (pirate) return;
        var content = LogContent.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Select(x => x[15..]);
        var melonLoaderVersion = content.First(x => x.Contains("MelonLoader")).Substring(13, 5);
        if (melonLoaderVersion != "0.5.7")
        {
            _logger.Information("Incorrect MelonLoader Version: {MelonLoaderVersion}", melonLoaderVersion);
            await MessageBoxService.CreateErrorMessageBox(string.Format(MsgBox_Content_IncorrectMelonLoaderVersion.Localize(),
                melonLoaderVersion));
        }
    }

    [RelayCommand]
    private async Task OpenLogFolder() => await _localService.OpenLogFolder();

    private void StartLogFileMonitor()
    {
        _watcher.Path = _settingService.Settings.MelonLoaderFolder;
        _watcher.Filter = "Latest.log";
        _watcher.Renamed += (_, _) => Initialize();
        _watcher.Changed += (_, _) => Initialize();
        _watcher.Created += (_, _) => Initialize();
        _watcher.Deleted += (_, _) => Initialize();
        _watcher.EnableRaisingEvents = true;
        _logger.Information("Log File Monitor Started");
    }
}