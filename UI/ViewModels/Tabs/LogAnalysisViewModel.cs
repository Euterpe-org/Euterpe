using System.Diagnostics;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.ViewModels.Tabs;

public partial class LogAnalysisViewModel : ViewModelBase, ILogAnalysisViewModel
{
    private readonly ILogAnalyzeService _logAnalyzeService;
    private readonly ILogger _logger;
    [ObservableProperty] private string _logContent;

    [UsedImplicitly]
    public ILocalService LocalService { get; init; }

    public LogAnalysisViewModel(ILogAnalyzeService logAnalyzeService, ILogger logger)
    {
        _logAnalyzeService = logAnalyzeService;
        _logger = logger;
        Initialize().ConfigureAwait(false);
    }

    public async Task Initialize()
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
        await _logAnalyzeService.AnalyzeLog();
    }

    [RelayCommand]
    private async Task OpenLogFolder() => await LocalService.OpenLogFolder();

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
}