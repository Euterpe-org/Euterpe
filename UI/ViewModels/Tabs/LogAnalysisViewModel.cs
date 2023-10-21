using System.Diagnostics;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.ViewModels.Tabs;

public partial class LogAnalysisViewModel : ViewModelBase, ILogAnalysisViewModel
{
    [ObservableProperty] private string _logContent;

    [UsedImplicitly]
    public ILocalService LocalService { get; init; }

    [UsedImplicitly]
    public ILogAnalyzeService LogAnalyzeService { get; init; }

    [UsedImplicitly]
    public ILogger Logger { get; init; }

    public async Task Initialize()
    {
        LogContent = await LogAnalyzeService.LoadLogAsync();
        Logger.Information("Log Analysis Window Initialized");
    }

    [RelayCommand]
    private async Task AnalyzeLogAsync()
    {
        Logger.Information("Log Analysis Started...");
        if (await LogAnalyzeService.CheckPirateAsync()) return;
        if (!await LogAnalyzeService.CheckMelonLoaderVersionAsync()) return;
        await LogAnalyzeService.AnalyzeLogAsync();
    }

    [RelayCommand]
    private async Task OpenLogFolderAsync() => await LocalService.OpenLogFolderAsync();

    [RelayCommand]
    private void OpenUrl(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
        Logger.Information("Open Url: {Url}", path);
    }
}