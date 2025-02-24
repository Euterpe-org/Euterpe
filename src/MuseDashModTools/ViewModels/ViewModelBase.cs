using System.Reactive.Linq;

namespace MuseDashModTools.ViewModels;

public partial class ViewModelBase : ObservableObject, IActivatableViewModel
{
    #region Injections

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    #endregion Injections

    public ViewModelBase()
    {
        this.WhenActivated(disposables =>
        {
            Observable.FromAsync(() => OnActivatedAsync(disposables))
                .Subscribe()
                .DisposeWith(disposables);

            Disposable.Create(OnDeactivated)
                .DisposeWith(disposables);
        });
    }

    public ViewModelActivator Activator { get; } = new();

    protected virtual Task OnActivatedAsync(CompositeDisposable disposables) => Task.CompletedTask;

    protected virtual void OnDeactivated()
    {
    }

    [RelayCommand]
    private Task OpenFileAsync(string filePath) => PlatformService.OpenFileAsync(filePath);

    [RelayCommand]
    private Task OpenFolderAsync(string folderPath) => PlatformService.OpenFolderAsync(folderPath);

    [RelayCommand]
    private Task OpenUrlAsync(string url) => PlatformService.OpenUriAsync(url);
}