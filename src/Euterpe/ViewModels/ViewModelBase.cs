namespace Euterpe.ViewModels;

public abstract partial class ViewModelBase : ObservableObject, IAsyncInitializable
{
    private readonly Lazy<Task> _initialization;

    #region Injections

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    #endregion Injections

    protected ViewModelBase() => _initialization = new Lazy<Task>(OnInitializeAsync);

    public Task InitializeAsync() => _initialization.Value;

    protected virtual Task OnInitializeAsync() => Task.CompletedTask;

    [RelayCommand]
    private Task OpenFileAsync(string filePath) => PlatformService.OpenFileAsync(filePath);

    [RelayCommand]
    private Task OpenFolderAsync(string folderPath) => PlatformService.OpenFolderAsync(folderPath);

    [RelayCommand]
    private Task OpenUrlAsync(string url) => PlatformService.OpenUriAsync(url);
}