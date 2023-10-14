#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace MuseDashModToolsUI.ViewModels.Tabs;

public partial class InfoJsonViewModel : ViewModelBase, IInfoJsonViewModel
{
    [ObservableProperty] private InfoJson _infoJson = new();

    [UsedImplicitly]
    public IChartService ChartService { get; init; }

    [UsedImplicitly]
    public IMessageBoxService MessageBoxService { get; init; }

    [RelayCommand]
    private async Task OnChooseChartFolder() => await ChartService.OnChooseChartFolder();
}