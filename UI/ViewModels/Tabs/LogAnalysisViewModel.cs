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
    private readonly ILogAnalyzeService _logAnalyzeService;
    private readonly ILogger _logger;
    [ObservableProperty] private string _logContent;
    public ILocalService LocalService { get; init; }
    public IMessageBoxService MessageBoxService { get; init; }

    public LogAnalysisViewModel(ILogAnalyzeService logAnalyzeService, ILogger logger)
    {
        _logAnalyzeService = logAnalyzeService;
        _logger = logger;
        Initialize();
    }

    public async void Initialize()
    {
        LogContent = await _logAnalyzeService.LoadLog();
        _logger.Information("Log Analysis Window Initialized");
    }

    [RelayCommand]
    private async Task AnalyzeLog()
    {
        _logger.Information("Log Analysis Started...");
        if (await _logAnalyzeService.CheckPirate()) return;
        if (!await _logAnalyzeService.CheckMelonLoaderVersion()) return;
    }

    [RelayCommand]
    private async Task OpenLogFolder() => await LocalService.OpenLogFolder();
}