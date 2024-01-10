#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace MuseDashModToolsUI.ViewModels.Pages;

public sealed partial class InfoJsonViewModel : ViewModelBase, IInfoJsonViewModel
{
    [ObservableProperty] private string[] _hideBmsDifficulty;
    [ObservableProperty] private string[] _hideBmsMode;
    [ObservableProperty] private InfoJson _infoJson = new();
    [ObservableProperty] private int _selectedHideBmsDifficulty;
    [ObservableProperty] private int _selectedHideBmsMode;

    [UsedImplicitly]
    public IInfoJsonService InfoJsonService { get; init; }

    [UsedImplicitly]
    public ILogger Logger { get; init; }

    [UsedImplicitly]
    public IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public ISerializationService SerializationService { get; init; }

    public void Initialize()
    {
        InfoJsonService.Initialize(InfoJson);
        /*Logger.Information("InfoJsonViewModel Initialized");*/
    }

    [RelayCommand]
    private async Task OnChooseChartFolderAsync() => await InfoJsonService.OnChooseChartFolderAsync();
}