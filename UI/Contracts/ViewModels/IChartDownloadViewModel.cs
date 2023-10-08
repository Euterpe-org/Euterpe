namespace MuseDashModToolsUI.Contracts.ViewModels;

public interface IChartDownloadViewModel
{
    string[] SortOptions { get; set; }
    int CurrentSortOptionIndex { get; set; }
    ChartSortOptions SortOption { get; }
    Task Initialize();
}