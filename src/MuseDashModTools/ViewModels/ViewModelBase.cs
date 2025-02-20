namespace MuseDashModTools.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    #region Injections

    [UsedImplicitly]
    public IPlatformService PlatformService { get; init; } = null!;

    #endregion Injections

    [RelayCommand]
    private Task OpenFileAsync(string filePath) => PlatformService.OpenFileAsync(filePath);

    [RelayCommand]
    private Task OpenFolderAsync(string folderPath) => PlatformService.OpenFolderAsync(folderPath);

    [RelayCommand]
    private Task OpenUrlAsync(string url) => PlatformService.OpenUriAsync(url);
}