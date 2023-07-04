using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using Serilog;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.ViewModels.Tabs;

public partial class LogAnalysisViewModel : ViewModelBase, ILogAnalysisViewModel
{
    private readonly ILogger _logger;
    private readonly ISettingService _settingService;
    [ObservableProperty] private string _logContent;
    private string _logPath;
    public ILocalService LocalService { get; init; }
    public IMessageBoxService MessageBoxService { get; init; }

    public LogAnalysisViewModel(ILogger logger, ISettingService settingService)
    {
        _logger = logger;
        _settingService = settingService;
        Initialize();
    }

    private void Initialize()
    {
        _logPath = Path.Combine(_settingService.Settings.MuseDashFolder!, "MelonLoader", "Latest.log");
        if (File.Exists(_logPath))
            LogContent = File.ReadAllText(_logPath);
        _logger.Information("Log Analysis Window Initialize");
    }

    [RelayCommand]
    private async Task AnalyzeLog()
    {
        var pirate = await LocalService.CheckPirate(LogContent);
        if (pirate) return;
    }

    [RelayCommand]
    private async Task OpenLogFolder() => await LocalService.OpenLogFolder();
}