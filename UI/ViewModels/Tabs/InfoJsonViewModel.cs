#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace MuseDashModToolsUI.ViewModels.Tabs;

public partial class InfoJsonViewModel : ViewModelBase, IInfoJsonViewModel
{
    [ObservableProperty] private string _author;
    [ObservableProperty] private string _bpm;
    [ObservableProperty] private string _hideBmsDifficulty;
    [ObservableProperty] private string _hideBmsMessage;
    [ObservableProperty] private string _hideBmsMode;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _scene;
    [ObservableProperty] private string _searchTags;

    [UsedImplicitly]
    public IChartService ChartService { get; init; }

    [UsedImplicitly]
    public IMessageBoxService MessageBoxService { get; init; }

    [RelayCommand]
    private async Task OnChooseChartFolder() => await ChartService.OnChooseChartFolder();
}