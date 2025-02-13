using Avalonia.Platform.Storage;

namespace MuseDashModTools.ViewModels;

public partial class PageViewModelBase : NavViewModelBase
{
    [UsedImplicitly]
    public TopLevel TopLevel { get; init; } = null!;

    [RelayCommand]
    private void OpenFolder(string folderPath) => TopLevel.Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(folderPath));
}